using System;
using System.Collections.Generic;
using TopView.Components;
using TopView.Entities;
using TopView.Requests;
using TopView.Utilities;
using UnityEngine;

namespace TopView.Systems
{
    // 근접 판정 생성 요청을 받아 저장소의 빈 칸을 채운다. 틱 마지막에 부른다.
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
                if (!SpawnUtility.TryTakeFreeIndex(melees.freeIndices, out int index))
                {
                    droppedCount++;
                    continue;
                }

                Spawn(ref melees.entities[index], in request);
            }

            if (droppedCount > 0)
            {
                Debug.LogWarning($"MeleeSpawnSystem >> 근접 판정 저장소에 빈 칸이 없어 생성 요청 {droppedCount}개를 버렸다.");
            }
        }

        // 칸 전체를 새 값으로 바꿔 이전 값이 남지 않게 한다. 맞힌 대상 배열만 비워서 다시 쓴다.
        private static void Spawn(ref MeleeEntity entity, in MeleeSpawnRequest request)
        {
            int generation = entity.generation + 1;
            EntityIdentifier[] hitTargets = entity.hits.targets;
            Array.Clear(hitTargets, 0, hitTargets.Length);

            entity = new MeleeEntity
            {
                isAlive = true,
                generation = generation,
                position = new PositionComponent { previous = request.center, current = request.center },
                melee = new MeleeComponent
                {
                    owner = request.owner,
                    radius = request.radius,
                    durationTicks = request.durationTicks
                },
                hits = new MeleeHitsComponent { targets = hitTargets },
                payload = new AttackPayloadComponent { faction = request.faction, damage = request.damage }
            };
        }
    }
}
