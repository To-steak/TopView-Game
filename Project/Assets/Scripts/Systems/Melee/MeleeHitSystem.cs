using System.Collections.Generic;
using TopView.Components;
using TopView.Entities;
using TopView.Requests;
using TopView.Utilities;
using UnityEngine;

namespace TopView.Systems
{
    // 근접 판정을 XZ 평면의 원으로 판정해 피해 요청을 넣고, 수명이 다한 판정의 소멸을 요청한다. 공격의 진영이 맞힐 쪽의 목록과 피해 요청 큐를 정한다.
    public static class MeleeHitSystem
    {
        // 처리할 근접 판정이 가진 컴포넌트
        private const ComponentMask MELEE_MASK = ComponentMask.Position | ComponentMask.Melee | ComponentMask.MeleeHits | ComponentMask.AttackPayload;

        /// <summary>
        /// 근접 판정마다 원 안에서 아직 맞지 않은 대상 모두에게 피해 요청을 넣고, 판정 시간을 1틱 늘려 다 되면 소멸을 요청한다.
        /// </summary>
        /// <param name="storage">엔티티 저장소. Position, Melee, MeleeHits, AttackPayload를 가진 칸만 처리하고, melee의 진행 시간과 meleeHits를 고친다</param>
        /// <param name="playerHitTargets">플레이어 피격 대상 목록</param>
        /// <param name="playerHitTargetCount">플레이어 피격 대상 목록에서 이번 틱에 채운 대상 수</param>
        /// <param name="enemyHitTargets">적 피격 대상 목록</param>
        /// <param name="enemyHitTargetCount">적 피격 대상 목록에서 이번 틱에 채운 대상 수</param>
        /// <param name="playerDamageRequests">플레이어 피해 요청 큐</param>
        /// <param name="enemyDamageRequests">적 피해 요청 큐</param>
        /// <param name="despawnRequests">근접 판정 소멸 요청 큐</param>
        public static void Tick(EntityStorage storage, HitTargetComponent[] playerHitTargets, int playerHitTargetCount, HitTargetComponent[] enemyHitTargets, int enemyHitTargetCount, Queue<DamageRequest> playerDamageRequests, Queue<DamageRequest> enemyDamageRequests, Queue<MeleeDespawnRequest> despawnRequests)
        {
            int droppedHitCount = 0;
            for (int index = 0; index < storage.entities.Length; index++)
            {
                ref Entity entity = ref storage.entities[index];
                if ((entity.mask & MELEE_MASK) != MELEE_MASK)
                {
                    continue;
                }

                if (entity.payload.faction == Faction.Player)
                {
                    droppedHitCount += Hit(in entity.melee, ref entity.meleeHits, entity.position.current, in entity.payload, enemyHitTargets, enemyHitTargetCount, enemyDamageRequests);
                }
                else
                {
                    droppedHitCount += Hit(in entity.melee, ref entity.meleeHits, entity.position.current, in entity.payload, playerHitTargets, playerHitTargetCount, playerDamageRequests);
                }

                entity.melee.elapsedTicks++;
                if (entity.melee.elapsedTicks >= entity.melee.durationTicks)
                {
                    despawnRequests.Enqueue(new MeleeDespawnRequest { target = new EntityIdentifier { index = index, generation = entity.generation } });
                }
            }

            if (droppedHitCount > 0)
            {
                Debug.LogWarning($"MeleeHitSystem >> 근접 판정에 맞은 대상을 기록할 칸이 모자라 대상 {droppedHitCount}개를 맞히지 않았다.");
            }
        }

        // 원 안에서 아직 맞지 않은 대상을 맞힌다. 기록할 칸이 모자라 건너뛴 수를 돌려준다.
        private static int Hit(in MeleeComponent melee, ref MeleeHitsComponent hits, Vector3 center, in AttackPayloadComponent payload, HitTargetComponent[] hitTargets, int hitTargetCount, Queue<DamageRequest> damageRequests)
        {
            int droppedCount = 0;
            for (int targetIndex = 0; targetIndex < hitTargetCount; targetIndex++)
            {
                ref readonly HitTargetComponent hitTarget = ref hitTargets[targetIndex];
                if (!TargetingUtility.IsWithin(center, hitTarget.position, melee.radius + hitTarget.radius)
                    || IsAlreadyHit(in hits, in hitTarget.target))
                {
                    continue;
                }

                if (hits.count >= hits.targets.Length)
                {
                    droppedCount++;
                    continue;
                }

                hits.targets[hits.count] = hitTarget.target;
                hits.count++;
                damageRequests.Enqueue(new DamageRequest { target = hitTarget.target, amount = payload.damage });
            }

            return droppedCount;
        }

        private static bool IsAlreadyHit(in MeleeHitsComponent hits, in EntityIdentifier target)
        {
            for (int hitIndex = 0; hitIndex < hits.count; hitIndex++)
            {
                if (IsSame(in hits.targets[hitIndex], in target))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsSame(in EntityIdentifier left, in EntityIdentifier right)
        {
            return left.index == right.index && left.generation == right.generation;
        }
    }
}
