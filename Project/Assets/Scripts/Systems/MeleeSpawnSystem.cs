using System;
using System.Collections.Generic;
using TopView.Components;
using TopView.Entities;
using TopView.Requests;
using UnityEngine;

namespace TopView.Systems
{
    // 근접 판정 생성 요청을 받아 공유 저장소의 근접 판정 구간 빈 칸에 컴포넌트를 채운다. 틱 마지막에 부른다.
    public static class MeleeSpawnSystem
    {
        /// <summary>
        /// 근접 판정 생성 요청을 모두 처리한다. 빈 칸이 없으면 요청을 버린다.
        /// </summary>
        /// <param name="entities">공유 저장소</param>
        /// <param name="meleeSpawnRequests">근접 판정 생성 요청 큐</param>
        public static void Tick(EntityStorage entities, Queue<MeleeSpawnRequest> meleeSpawnRequests)
        {
            int droppedCount = 0;
            while (meleeSpawnRequests.Count > 0)
            {
                MeleeSpawnRequest request = meleeSpawnRequests.Dequeue();
                if (!SpawnSystem.TryTakeFreeIndex(entities.freeMeleeIndices, out int index))
                {
                    droppedCount++;
                    continue;
                }

                EntityReference[] hitTargets = entities.meleeHits[index].targets;
                Array.Clear(hitTargets, 0, hitTargets.Length);

                SpawnSystem.ClaimSlot(entities, index, ComponentMask.MeleeArchetype);
                entities.attackPayloads[index] = new AttackPayloadComponent { faction = request.faction, damage = request.damage };
                entities.positions[index] = new PositionComponent { previous = request.center, current = request.center };
                entities.melees[index] = new MeleeComponent
                {
                    owner = request.owner,
                    radius = request.radius,
                    durationTicks = request.durationTicks
                };
                entities.meleeHits[index] = new MeleeHitsComponent { targets = hitTargets };
            }

            if (droppedCount > 0)
            {
                Debug.LogWarning($"MeleeSpawnSystem >> 근접 판정 저장소에 빈 칸이 없어 생성 요청 {droppedCount}개를 버렸다.");
            }
        }
    }
}
