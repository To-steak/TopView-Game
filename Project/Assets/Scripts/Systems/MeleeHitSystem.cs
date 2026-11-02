using System.Collections.Generic;
using TopView.Components;
using TopView.Entities;
using TopView.Requests;
using UnityEngine;

namespace TopView.Systems
{
    // 근접 판정을 XZ 평면의 원으로 판정해 피해 요청을 넣고, 수명이 다한 판정을 없앤다. 공격의 진영이 맞힐 쪽의 목록과 피해 요청 큐를 정한다.
    public static class MeleeHitSystem
    {
        /// <summary>
        /// 근접 판정마다 원 안에서 아직 맞지 않은 대상 모두에게 피해 요청을 넣고, 판정 시간을 1틱 늘려 다 되면 없앤다.
        /// </summary>
        /// <param name="melees">근접 판정 저장소</param>
        /// <param name="playerHitTargets">플레이어 피격 대상 목록</param>
        /// <param name="playerHitTargetCount">플레이어 피격 대상 목록에서 이번 틱에 채운 대상 수</param>
        /// <param name="enemyHitTargets">적 피격 대상 목록</param>
        /// <param name="enemyHitTargetCount">적 피격 대상 목록에서 이번 틱에 채운 대상 수</param>
        /// <param name="playerDamageRequests">플레이어 피해 요청 큐</param>
        /// <param name="enemyDamageRequests">적 피해 요청 큐</param>
        public static void Tick(MeleeStorage melees, HitTargetComponent[] playerHitTargets, int playerHitTargetCount, HitTargetComponent[] enemyHitTargets, int enemyHitTargetCount, Queue<DamageRequest> playerDamageRequests, Queue<DamageRequest> enemyDamageRequests)
        {
            int droppedHitCount = 0;
            for (int index = 0; index < melees.entities.Length; index++)
            {
                ref MeleeEntity entity = ref melees.entities[index];
                if (!entity.isAlive)
                {
                    continue;
                }

                if (entity.payload.faction == Faction.Player)
                {
                    droppedHitCount += Hit(in entity.melee, ref entity.hits, entity.position.current, in entity.payload, enemyHitTargets, enemyHitTargetCount, enemyDamageRequests);
                }
                else
                {
                    droppedHitCount += Hit(in entity.melee, ref entity.hits, entity.position.current, in entity.payload, playerHitTargets, playerHitTargetCount, playerDamageRequests);
                }

                entity.melee.elapsedTicks++;
                if (entity.melee.elapsedTicks >= entity.melee.durationTicks)
                {
                    Despawn(melees, index);
                }
            }

            if (droppedHitCount > 0)
            {
                Debug.LogWarning($"MeleeHitSystem >> 근접 판정에 맞은 대상을 기록할 칸이 모자라 대상 {droppedHitCount}개를 맞히지 않았다.");
            }
        }

        /// <summary>
        /// 공격한 엔티티가 owner인 근접 판정을 모두 없앤다. 공격한 쪽이 사망할 때 부른다.
        /// 저장소마다 칸 번호를 따로 쓰므로 EntityIdentifier만으로는 다른 진영의 엔티티와 겹칠 수 있어 진영도 함께 비교한다.
        /// </summary>
        /// <param name="melees">근접 판정 저장소</param>
        /// <param name="owner">공격한 엔티티</param>
        /// <param name="ownerFaction">공격한 엔티티의 진영</param>
        public static void DespawnOwnedBy(MeleeStorage melees, in EntityIdentifier owner, Faction ownerFaction)
        {
            for (int index = 0; index < melees.entities.Length; index++)
            {
                ref readonly MeleeEntity entity = ref melees.entities[index];
                if (entity.isAlive && entity.payload.faction == ownerFaction && IsSame(in entity.melee.owner, in owner))
                {
                    Despawn(melees, index);
                }
            }
        }

        // 엔티티를 즉시 없애고 칸을 반납한다.
        private static void Despawn(MeleeStorage melees, int index)
        {
            melees.entities[index].isAlive = false;
            melees.freeIndices.Add(index);
        }

        // 원 안에서 아직 맞지 않은 대상을 맞힌다. 기록할 칸이 모자라 건너뛴 수를 돌려준다.
        private static int Hit(in MeleeComponent melee, ref MeleeHitsComponent hits, Vector3 center, in AttackPayloadComponent payload, HitTargetComponent[] hitTargets, int hitTargetCount, Queue<DamageRequest> damageRequests)
        {
            int droppedCount = 0;
            for (int targetIndex = 0; targetIndex < hitTargetCount; targetIndex++)
            {
                ref readonly HitTargetComponent hitTarget = ref hitTargets[targetIndex];
                if (!TargetingSystem.IsWithin(center, hitTarget.position, melee.radius + hitTarget.radius)
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
