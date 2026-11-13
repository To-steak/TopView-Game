using System.Collections.Generic;
using TopView.Components;
using TopView.Entities;
using TopView.Requests;
using TopView.Utilities;
using UnityEngine;

namespace TopView.Systems
{
    // 전리품 생성 요청을 받아 저장소 전리품 구간의 빈 칸을 채운다. 틱 마지막에 부른다.
    public static class LootSpawnSystem
    {
        /// <summary>
        /// 전리품 생성 요청을 모두 처리한다. 빈 칸이 없으면 요청을 버린다.
        /// </summary>
        /// <param name="storage">엔티티 저장소. 전리품 구간의 칸을 채운다</param>
        /// <param name="lootSpawnRequests">전리품 생성 요청 큐</param>
        /// <param name="lifetimeTicks">전리품이 생긴 뒤 사라지기까지의 시간(틱)</param>
        public static void Tick(EntityStorage storage, Queue<LootSpawnRequest> lootSpawnRequests, int lifetimeTicks)
        {
            int droppedCount = 0;
            while (lootSpawnRequests.Count > 0)
            {
                LootSpawnRequest request = lootSpawnRequests.Dequeue();
                if (!SpawnUtility.TryTakeFreeIndex(storage.freeLootIndices, out int index))
                {
                    droppedCount++;
                    continue;
                }

                Spawn(ref storage.entities[index], in request, lifetimeTicks);
            }

            if (droppedCount > 0)
            {
                Debug.LogWarning($"LootSpawnSystem >> 전리품 구간에 빈 칸이 없어 생성 요청 {droppedCount}개를 버렸다.");
            }
        }

        // 칸 전체를 새 값으로 바꿔 이전 값이 남지 않게 한다.
        private static void Spawn(ref Entity entity, in LootSpawnRequest request, int lifetimeTicks)
        {
            int generation = entity.generation + 1;

            entity = new Entity
            {
                mask = ComponentMask.Position | ComponentMask.Loot,
                generation = generation,
                loot = new LootComponent { kind = request.kind, amount = request.amount, remainingTicks = lifetimeTicks },
                position = new PositionComponent { previous = request.position, current = request.position }
            };
        }
    }
}
