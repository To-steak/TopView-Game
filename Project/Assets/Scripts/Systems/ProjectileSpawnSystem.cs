using System.Collections.Generic;
using TopView.Components;
using TopView.Entities;
using TopView.Requests;
using UnityEngine;

namespace TopView.Systems
{
    // 투사체 생성 요청을 받아 저장소의 빈 칸을 채운다. 틱 마지막에 부른다.
    public static class ProjectileSpawnSystem
    {
        /// <summary>
        /// 투사체 생성 요청을 모두 처리한다. 빈 칸이 없으면 요청을 버린다.
        /// </summary>
        /// <param name="projectiles">투사체 저장소</param>
        /// <param name="projectileSpawnRequests">투사체 생성 요청 큐</param>
        public static void Tick(ProjectileStorage projectiles, Queue<ProjectileSpawnRequest> projectileSpawnRequests)
        {
            int droppedCount = 0;
            while (projectileSpawnRequests.Count > 0)
            {
                ProjectileSpawnRequest request = projectileSpawnRequests.Dequeue();
                if (!SpawnUtility.TryTakeFreeIndex(projectiles.freeIndices, out int index))
                {
                    droppedCount++;
                    continue;
                }

                Spawn(ref projectiles.entities[index], in request);
            }

            if (droppedCount > 0)
            {
                Debug.LogWarning($"ProjectileSpawnSystem >> 투사체 저장소에 빈 칸이 없어 생성 요청 {droppedCount}개를 버렸다.");
            }
        }

        // 칸 전체를 새 값으로 바꿔 이전 값이 남지 않게 한다.
        private static void Spawn(ref ProjectileEntity entity, in ProjectileSpawnRequest request)
        {
            int generation = entity.generation + 1;

            entity = new ProjectileEntity
            {
                isAlive = true,
                generation = generation,
                projectile = request.projectile,
                motion = new ProjectileMotionComponent { velocity = request.velocity },
                position = new PositionComponent { previous = request.position, current = request.position },
                rotation = new RotationComponent { previous = request.rotation, current = request.rotation },
                payload = new AttackPayloadComponent { faction = request.faction, damage = request.damage }
            };
        }
    }
}
