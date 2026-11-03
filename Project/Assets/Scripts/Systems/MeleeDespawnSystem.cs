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
        /// <param name="melees">근접 판정 저장소</param>
        /// <param name="despawnRequests">근접 판정 소멸 요청 큐</param>
        public static void Tick(MeleeStorage melees, Queue<MeleeDespawnRequest> despawnRequests)
        {
            while (despawnRequests.Count > 0)
            {
                EntityIdentifier target = despawnRequests.Dequeue().target;
                ref MeleeEntity entity = ref melees.entities[target.index];
                if (entity.isAlive && entity.generation == target.generation)
                {
                    entity.isAlive = false;
                    melees.freeIndices.Add(target.index);
                }
            }
        }
    }
}
