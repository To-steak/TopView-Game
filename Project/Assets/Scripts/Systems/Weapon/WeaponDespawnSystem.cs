using System.Collections.Generic;
using TopView.Entities;
using TopView.Requests;

namespace TopView.Systems
{
    // 소멸 요청을 받은 무기를 없애고 칸을 반납한다.
    public static class WeaponDespawnSystem
    {
        // 무기가 가진 컴포넌트
        private const ComponentMask WEAPON_MASK = ComponentMask.Owner | ComponentMask.Weapon;

        /// <summary>
        /// 무기 소멸 요청을 모두 처리한다. 이미 사라졌거나 칸이 다시 채워진 대상은 건너뛴다.
        /// </summary>
        /// <param name="storage">엔티티 저장소. Owner와 Weapon을 가진 칸만 비우고, 무기 구간의 빈 칸 목록에 돌려준다</param>
        /// <param name="despawnRequests">무기 소멸 요청 큐</param>
        public static void Tick(EntityStorage storage, Queue<WeaponDespawnRequest> despawnRequests)
        {
            while (despawnRequests.Count > 0)
            {
                EntityIdentifier target = despawnRequests.Dequeue().target;
                ref Entity entity = ref storage.entities[target.index];
                if ((entity.mask & WEAPON_MASK) == WEAPON_MASK && entity.generation == target.generation)
                {
                    entity.mask = ComponentMask.None;
                    storage.freeWeaponIndices.Add(target.index);
                }
            }
        }
    }
}
