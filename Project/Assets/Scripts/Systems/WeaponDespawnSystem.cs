using System.Collections.Generic;
using TopView.Entities;
using TopView.Requests;

namespace TopView.Systems
{
    // 소멸 요청을 받은 무기를 없애고 칸을 반납한다.
    public static class WeaponDespawnSystem
    {
        /// <summary>
        /// 무기 소멸 요청을 모두 처리한다. 이미 사라졌거나 칸이 다시 채워진 대상은 건너뛴다.
        /// </summary>
        /// <param name="weapons">무기 저장소</param>
        /// <param name="despawnRequests">무기 소멸 요청 큐</param>
        public static void Tick(WeaponStorage weapons, Queue<WeaponDespawnRequest> despawnRequests)
        {
            while (despawnRequests.Count > 0)
            {
                EntityIdentifier target = despawnRequests.Dequeue().target;
                ref WeaponEntity entity = ref weapons.entities[target.index];
                if (entity.isAlive && entity.generation == target.generation)
                {
                    entity.isAlive = false;
                    weapons.freeIndices.Add(target.index);
                }
            }
        }
    }
}
