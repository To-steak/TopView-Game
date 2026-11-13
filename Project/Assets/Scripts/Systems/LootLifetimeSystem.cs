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
        /// <param name="storage">엔티티 저장소. Loot를 가진 칸만 처리한다</param>
        /// <param name="despawnRequests">전리품 소멸 요청 큐</param>
        public static void Tick(EntityStorage storage, Queue<LootDespawnRequest> despawnRequests)
        {
            for (int index = 0; index < storage.entities.Length; index++)
            {
                ref Entity entity = ref storage.entities[index];
                if ((entity.mask & ComponentMask.Loot) != 0 && Elapse(ref entity.loot))
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
