using System.Collections.Generic;
using TopView.Components;
using TopView.Entities;
using TopView.Requests;
using UnityEngine;

namespace TopView.Systems
{
    // 투사체 생성 요청을 받아 공유 저장소의 투사체 구간 빈 칸에 컴포넌트를 채운다. 틱 마지막에 부른다.
    public static class ProjectileSpawnSystem
    {
        /// <summary>
        /// 투사체 생성 요청을 모두 처리한다. 빈 칸이 없으면 요청을 버린다.
        /// </summary>
        /// <param name="entities">공유 저장소</param>
        /// <param name="projectileSpawnRequests">투사체 생성 요청 큐</param>
        public static void Tick(EntityStorage entities, Queue<ProjectileSpawnRequest> projectileSpawnRequests)
        {
            int droppedCount = 0;
            while (projectileSpawnRequests.Count > 0)
            {
                ProjectileSpawnRequest request = projectileSpawnRequests.Dequeue();
                if (!SpawnSystem.TryTakeFreeIndex(entities.freeProjectileIndices, out int index))
                {
                    droppedCount++;
                    continue;
                }

                SpawnSystem.ClaimSlot(entities, index, ComponentMask.ProjectileArchetype);
                entities.projectiles[index] = request.projectile;
                entities.attackPayloads[index] = new AttackPayloadComponent { faction = request.faction, damage = request.damage };
                entities.positions[index] = new PositionComponent { previous = request.position, current = request.position };
                entities.rotations[index] = new RotationComponent { previous = request.rotation, current = request.rotation };
                entities.projectileMotions[index] = new ProjectileMotionComponent { velocity = request.velocity };
            }

            if (droppedCount > 0)
            {
                Debug.LogWarning($"ProjectileSpawnSystem >> 투사체 저장소에 빈 칸이 없어 생성 요청 {droppedCount}개를 버렸다.");
            }
        }
    }
}
