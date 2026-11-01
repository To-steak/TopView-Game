using System.Collections.Generic;
using TopView.Entities;
using TopView.Requests;
using UnityEngine;

namespace TopView.Systems
{
    // 투사체 생성 요청을 받아 ProjectileStorage의 빈 칸에 엔티티를 채운다. 틱 마지막에 부른다.
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
                if (!SpawnSystem.TryTakeFreeIndex(projectiles.freeIndices, out int index))
                {
                    droppedCount++;
                    continue;
                }

                ref ProjectileEntity slot = ref projectiles.entities[index];
                slot = new ProjectileEntity
                {
                    isAlive = true,
                    generation = slot.generation + 1,
                    projectile = request.projectile,
                    isTargetingPlayer = request.isTargetingPlayer,
                    damage = request.damage,
                    previousPosition = request.position,
                    currentPosition = request.position,
                    rotation = request.rotation,
                    velocity = request.velocity
                };
            }

            if (droppedCount > 0)
            {
                Debug.LogWarning($"ProjectileSpawnSystem >> 투사체 저장소에 빈 칸이 없어 생성 요청 {droppedCount}개를 버렸다.");
            }
        }
    }
}
