using System.Collections.Generic;
using TopView.Entities;
using TopView.Requests;

namespace TopView.Systems
{
    // 소멸 요청을 받은 근접 판정을 없애고 칸을 반납한다.
    public static class MeleeDespawnSystem
    {
        /// <summary>
        /// 쌓인 소멸 요청을 모두 처리한다. 이미 없어졌거나 칸이 재사용된 대상은 건너뛴다. 요청을 넣는 System 바로 뒤에 부른다.
        /// </summary>
        /// <param name="storage">엔티티 저장소. 비운 칸은 근접 판정 구간의 빈 칸 목록에 돌려준다</param>
        /// <param name="despawnRequests">근접 판정 소멸 요청 큐</param>
        public static void Tick(EntityStorage storage, Queue<MeleeDespawnRequest> despawnRequests)
        {
            while (despawnRequests.Count > 0)
            {
                EntityIdentifier target = despawnRequests.Dequeue().target;
                ref Entity entity = ref storage.entities[target.index];
                if ((entity.mask & ComponentMask.Melee) != 0 && entity.generation == target.generation)
                {
                    entity.mask = ComponentMask.None;
                    storage.freeMeleeIndices.Add(target.index);
                }
            }
        }
    }
}
