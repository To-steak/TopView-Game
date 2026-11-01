using System;
using System.Collections.Generic;
using TopView.Entities;
using TopView.Requests;
using UnityEngine;

namespace TopView.Systems
{
    // 근접 판정 생성 요청을 받아 MeleeStorage의 빈 칸에 엔티티를 채운다. 틱 마지막에 부른다.
    public static class MeleeSpawnSystem
    {
        /// <summary>
        /// 근접 판정 생성 요청을 모두 처리한다. 빈 칸이 없으면 요청을 버린다.
        /// </summary>
        /// <param name="melees">근접 판정 저장소</param>
        /// <param name="meleeSpawnRequests">근접 판정 생성 요청 큐</param>
        public static void Tick(MeleeStorage melees, Queue<MeleeSpawnRequest> meleeSpawnRequests)
        {
            int droppedCount = 0;
            while (meleeSpawnRequests.Count > 0)
            {
                MeleeSpawnRequest request = meleeSpawnRequests.Dequeue();
                if (!SpawnSystem.TryTakeFreeIndex(melees.freeIndices, out int index))
                {
                    droppedCount++;
                    continue;
                }

                ref MeleeEntity slot = ref melees.entities[index];
                EntityReference[] hitTargets = slot.hitTargets;
                Array.Clear(hitTargets, 0, hitTargets.Length);
                slot = new MeleeEntity
                {
                    isAlive = true,
                    generation = slot.generation + 1,
                    owner = request.owner,
                    faction = request.faction,
                    center = request.center,
                    radius = request.radius,
                    damage = request.damage,
                    durationTicks = request.durationTicks,
                    hitTargets = hitTargets
                };
            }

            if (droppedCount > 0)
            {
                Debug.LogWarning($"MeleeSpawnSystem >> 근접 판정 저장소에 빈 칸이 없어 생성 요청 {droppedCount}개를 버렸다.");
            }
        }
    }
}
