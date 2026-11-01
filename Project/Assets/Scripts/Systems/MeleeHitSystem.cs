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
        /// 살아 있는 근접 판정마다 원 안에서 아직 맞지 않은 대상 모두에게 피해 요청을 넣고, 판정 시간을 1틱 늘려 다 되면 없앤다.
        /// </summary>
        /// <param name="melees">근접 판정 저장소</param>
        /// <param name="hitTargets">피격 대상 목록</param>
        /// <param name="hitTargetCount">피격 대상 목록에서 이번 틱에 채운 대상 수</param>
        /// <param name="damageRequests">피해 요청 큐</param>
        public static void Tick(MeleeStorage melees, HitTargetComponent[] hitTargets, int hitTargetCount, Queue<DamageRequest> damageRequests)
        {
            int droppedHitCount = 0;
            for (int meleeIndex = 0; meleeIndex < melees.entities.Length; meleeIndex++)
            {
                ref MeleeEntity melee = ref melees.entities[meleeIndex];
                if (!melee.isAlive)
                {
                    continue;
                }

                droppedHitCount += Hit(ref melee, hitTargets, hitTargetCount, damageRequests);

                melee.elapsedTicks++;
                if (melee.elapsedTicks >= melee.durationTicks)
                {
                    Despawn(melees, meleeIndex);
                }
            }

            if (droppedHitCount > 0)
            {
                Debug.LogWarning($"MeleeHitSystem >> 근접 판정에 맞은 대상을 기록할 칸이 모자라 대상 {droppedHitCount}개를 맞히지 않았다.");
            }
        }

        /// <summary>
        /// 근접 판정 하나를 없애고 칸을 반납한다.
        /// </summary>
        /// <param name="melees">근접 판정 저장소</param>
        /// <param name="meleeIndex">없앨 칸 번호</param>
        public static void Despawn(MeleeStorage melees, int meleeIndex)
        {
            melees.entities[meleeIndex].isAlive = false;
            melees.freeIndices.Add(meleeIndex);
        }

        /// <summary>
        /// 공격한 엔티티가 owner인 근접 판정을 모두 없앤다. 공격한 쪽이 사망할 때 부른다.
        /// </summary>
        /// <param name="melees">근접 판정 저장소</param>
        /// <param name="owner">공격한 엔티티</param>
        public static void DespawnOwnedBy(MeleeStorage melees, in EntityReference owner)
        {
            for (int meleeIndex = 0; meleeIndex < melees.entities.Length; meleeIndex++)
            {
                ref readonly MeleeEntity melee = ref melees.entities[meleeIndex];
                if (melee.isAlive && IsSame(in melee.owner, in owner))
                {
                    Despawn(melees, meleeIndex);
                }
            }
        }

        // 원 안에서 아직 맞지 않은 대상을 맞힌다. 기록할 칸이 모자라 건너뛴 수를 돌려준다.
        private static int Hit(ref MeleeEntity melee, HitTargetComponent[] hitTargets, int hitTargetCount, Queue<DamageRequest> damageRequests)
        {
            int droppedCount = 0;
            for (int targetIndex = 0; targetIndex < hitTargetCount; targetIndex++)
            {
                ref readonly HitTargetComponent hitTarget = ref hitTargets[targetIndex];
                if (hitTarget.faction == melee.faction
                    || !TargetingSystem.IsWithin(melee.center, hitTarget.position, melee.radius + hitTarget.radius)
                    || IsAlreadyHit(in melee, in hitTarget.target))
                {
                    continue;
                }

                if (melee.hitTargetCount >= melee.hitTargets.Length)
                {
                    droppedCount++;
                    continue;
                }

                melee.hitTargets[melee.hitTargetCount] = hitTarget.target;
                melee.hitTargetCount++;
                damageRequests.Enqueue(new DamageRequest { target = hitTarget.target, amount = melee.damage });
            }

            return droppedCount;
        }

        private static bool IsAlreadyHit(in MeleeEntity melee, in EntityReference target)
        {
            for (int hitIndex = 0; hitIndex < melee.hitTargetCount; hitIndex++)
            {
                if (IsSame(in melee.hitTargets[hitIndex], in target))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsSame(in EntityReference left, in EntityReference right)
        {
            return left.kind == right.kind
                && left.identifier.index == right.identifier.index
                && left.identifier.generation == right.identifier.generation;
        }
    }
}
