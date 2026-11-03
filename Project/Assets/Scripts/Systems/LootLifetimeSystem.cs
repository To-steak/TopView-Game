using System.Collections.Generic;
using TopView.Components;
using TopView.Entities;
using TopView.Requests;

namespace TopView.Systems
{
    // 전리품의 남은 수명을 줄이고, 수명이 끝나면 소멸을 요청한다.
    public static class LootLifetimeSystem
    {
        /// <summary>
        /// 살아 있는 전리품의 남은 수명을 1틱 줄인다. 0이 되면 소멸을 요청한다.
        /// </summary>
        /// <param name="loots">전리품 저장소</param>
        /// <param name="despawnRequests">전리품 소멸 요청 큐</param>
        public static void Tick(LootStorage loots, Queue<LootDespawnRequest> despawnRequests)
        {
            for (int index = 0; index < loots.entities.Length; index++)
            {
                ref LootEntity entity = ref loots.entities[index];
                if (entity.isAlive && Elapse(ref entity.loot))
                {
                    despawnRequests.Enqueue(new LootDespawnRequest
                    {
                        target = new EntityIdentifier { index = index, generation = entity.generation }
                    });
                }
            }
        }

        // 남은 수명을 줄인다. 수명이 끝났으면 true를 돌려준다.
        private static bool Elapse(ref LootComponent loot)
        {
            loot.remainingTicks--;
            return loot.remainingTicks <= 0;
        }
    }
}
