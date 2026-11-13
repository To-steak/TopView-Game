using System.Collections.Generic;
using TopView.Entities;
using TopView.Requests;

namespace TopView.Systems
{
    // 소멸 요청을 받은 투사체를 없애고 칸을 반납한다.
    public static class ProjectileDespawnSystem
    {
        /// <summary>
        /// 쌓인 직선 투사체 소멸 요청을 모두 처리한다. 이미 없어졌거나 칸이 재사용된 대상은 건너뛴다. 요청을 넣는 System 바로 뒤에 부른다.
        /// </summary>
        /// <param name="projectiles">직선 투사체 저장소</param>
        /// <param name="despawnRequests">직선 투사체 소멸 요청 큐</param>
        public static void Tick(StraightProjectileStorage projectiles, Queue<ProjectileDespawnRequest> despawnRequests)
        {
            while (despawnRequests.Count > 0)
            {
                EntityIdentifier target = despawnRequests.Dequeue().target;
                ref StraightProjectileEntity entity = ref projectiles.entities[target.index];
                if (entity.isAlive && entity.generation == target.generation)
                {
                    entity.isAlive = false;
                    projectiles.freeIndices.Add(target.index);
                }
            }
        }

        /// <summary>
        /// 쌓인 포물선 투사체 소멸 요청을 모두 처리한다. 이미 없어졌거나 칸이 재사용된 대상은 건너뛴다. 요청을 넣는 System 바로 뒤에 부른다.
        /// </summary>
        /// <param name="projectiles">포물선 투사체 저장소</param>
        /// <param name="despawnRequests">포물선 투사체 소멸 요청 큐</param>
        public static void Tick(ArcProjectileStorage projectiles, Queue<ProjectileDespawnRequest> despawnRequests)
        {
            while (despawnRequests.Count > 0)
            {
                EntityIdentifier target = despawnRequests.Dequeue().target;
                ref ArcProjectileEntity entity = ref projectiles.entities[target.index];
                if (entity.isAlive && entity.generation == target.generation)
                {
                    entity.isAlive = false;
                    projectiles.freeIndices.Add(target.index);
                }
            }
        }
    }
}
