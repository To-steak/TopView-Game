using System.Collections.Generic;
using TopView.Components;
using TopView.Entities;
using TopView.Requests;
using UnityEngine;

namespace TopView.Systems
{
    // 근접 판정을 XZ 평면의 원으로 판정해 피해 요청을 넣고, 수명이 다한 판정을 없앤다. 공격한 쪽과 진영이 다른 피격 대상만 맞는다.
    public static class MeleeHitSystem
    {
        /// <summary>
        /// 근접 판정마다 원 안에서 아직 맞지 않은 대상 모두에게 피해 요청을 넣고, 판정 시간을 1틱 늘려 다 되면 없앤다.
        /// </summary>
        /// <param name="masks">칸별 컴포넌트 표시</param>
        /// <param name="melees">근접 판정 값과 진행 시간</param>
        /// <param name="meleeHits">근접 판정이 이미 맞힌 대상</param>
        /// <param name="positions">위치. 근접 판정에서는 판정 원의 중심이다</param>
        /// <param name="payloads">공격의 진영과 피해량</param>
        /// <param name="freeMeleeIndices">근접 판정 구간의 빈 칸 번호 목록</param>
        /// <param name="hitTargets">피격 대상 목록</param>
        /// <param name="hitTargetCount">피격 대상 목록에서 이번 틱에 채운 대상 수</param>
        /// <param name="damageRequests">피해 요청 큐</param>
        public static void Tick(ComponentMask[] masks, MeleeComponent[] melees, MeleeHitsComponent[] meleeHits, PositionComponent[] positions, AttackPayloadComponent[] payloads, List<int> freeMeleeIndices, HitTargetComponent[] hitTargets, int hitTargetCount, Queue<DamageRequest> damageRequests)
        {
            int droppedHitCount = 0;
            for (int index = 0; index < masks.Length; index++)
            {
                if ((masks[index] & ComponentMask.MeleeArchetype) != ComponentMask.MeleeArchetype)
                {
                    continue;
                }

                ref MeleeComponent melee = ref melees[index];
                droppedHitCount += Hit(in melee, ref meleeHits[index], positions[index].current, in payloads[index], hitTargets, hitTargetCount, damageRequests);

                melee.elapsedTicks++;
                if (melee.elapsedTicks >= melee.durationTicks)
                {
                    SpawnSystem.ReleaseSlot(masks, freeMeleeIndices, index);
                }
            }

            if (droppedHitCount > 0)
            {
                Debug.LogWarning($"MeleeHitSystem >> 근접 판정에 맞은 대상을 기록할 칸이 모자라 대상 {droppedHitCount}개를 맞히지 않았다.");
            }
        }

        /// <summary>
        /// 공격한 엔티티가 owner인 근접 판정을 모두 없앤다. 공격한 쪽이 사망할 때 부른다.
        /// </summary>
        /// <param name="masks">칸별 컴포넌트 표시</param>
        /// <param name="melees">근접 판정 값과 진행 시간</param>
        /// <param name="freeMeleeIndices">근접 판정 구간의 빈 칸 번호 목록</param>
        /// <param name="owner">공격한 엔티티</param>
        public static void DespawnOwnedBy(ComponentMask[] masks, MeleeComponent[] melees, List<int> freeMeleeIndices, in EntityIdentifier owner)
        {
            for (int index = 0; index < masks.Length; index++)
            {
                if ((masks[index] & ComponentMask.MeleeArchetype) == ComponentMask.MeleeArchetype && IsSame(in melees[index].owner, in owner))
                {
                    SpawnSystem.ReleaseSlot(masks, freeMeleeIndices, index);
                }
            }
        }

        // 원 안에서 아직 맞지 않은 대상을 맞힌다. 기록할 칸이 모자라 건너뛴 수를 돌려준다.
        private static int Hit(in MeleeComponent melee, ref MeleeHitsComponent hits, Vector3 center, in AttackPayloadComponent payload, HitTargetComponent[] hitTargets, int hitTargetCount, Queue<DamageRequest> damageRequests)
        {
            int droppedCount = 0;
            for (int targetIndex = 0; targetIndex < hitTargetCount; targetIndex++)
            {
                ref readonly HitTargetComponent hitTarget = ref hitTargets[targetIndex];
                if (hitTarget.faction == payload.faction
                    || !TargetingSystem.IsWithin(center, hitTarget.position, melee.radius + hitTarget.radius)
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
