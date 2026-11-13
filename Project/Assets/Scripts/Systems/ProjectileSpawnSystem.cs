using System.Collections.Generic;
using TopView.Components;
using TopView.Entities;
using TopView.Requests;
using TopView.Utilities;
using UnityEngine;

namespace TopView.Systems
{
    // 투사체 생성 요청을 받아 저장소의 빈 칸을 채운다. 틱 마지막에 부른다.
    public static class ProjectileSpawnSystem
    {
        /// <summary>
        /// 직선 투사체 생성 요청을 모두 처리한다. 빈 칸이 없으면 요청을 버린다.
        /// </summary>
        /// <param name="projectiles">직선 투사체 저장소</param>
        /// <param name="spawnRequests">직선 투사체 생성 요청 큐</param>
        public static void Tick(StraightProjectileStorage projectiles, Queue<StraightProjectileSpawnRequest> spawnRequests)
        {
            int droppedCount = 0;
            while (spawnRequests.Count > 0)
            {
                StraightProjectileSpawnRequest request = spawnRequests.Dequeue();
                if (!SpawnUtility.TryTakeFreeIndex(projectiles.freeIndices, out int index))
                {
                    droppedCount++;
                    continue;
                }

                Spawn(ref projectiles.entities[index], in request);
            }

            if (droppedCount > 0)
            {
                Debug.LogWarning($"ProjectileSpawnSystem >> 직선 투사체 저장소에 빈 칸이 없어 생성 요청 {droppedCount}개를 버렸다.");
            }
        }

        /// <summary>
        /// 포물선 투사체 생성 요청을 모두 처리한다. 빈 칸이 없거나 flightTicks가 0 이하면 요청을 버린다.
        /// </summary>
        /// <param name="projectiles">포물선 투사체 저장소</param>
        /// <param name="spawnRequests">포물선 투사체 생성 요청 큐</param>
        public static void Tick(ArcProjectileStorage projectiles, Queue<ArcProjectileSpawnRequest> spawnRequests)
        {
            int droppedCount = 0;
            int invalidFlightCount = 0;
            while (spawnRequests.Count > 0)
            {
                ArcProjectileSpawnRequest request = spawnRequests.Dequeue();
                if (request.projectile.flight.flightTicks <= 0)
                {
                    invalidFlightCount++;
                    continue;
                }
                if (!SpawnUtility.TryTakeFreeIndex(projectiles.freeIndices, out int index))
                {
                    droppedCount++;
                    continue;
                }

                Spawn(ref projectiles.entities[index], in request);
            }

            if (invalidFlightCount > 0)
            {
                Debug.LogWarning($"ProjectileSpawnSystem >> flightTicks가 0 이하인 포물선 투사체 생성 요청 {invalidFlightCount}개를 버렸다. flightTicks는 0보다 커야 한다.");
            }
            if (droppedCount > 0)
            {
                Debug.LogWarning($"ProjectileSpawnSystem >> 포물선 투사체 저장소에 빈 칸이 없어 생성 요청 {droppedCount}개를 버렸다.");
            }
        }

        // 칸 전체를 새 값으로 바꿔 이전 값이 남지 않게 한다.
        private static void Spawn(ref StraightProjectileEntity entity, in StraightProjectileSpawnRequest request)
        {
            int generation = entity.generation + 1;

            entity = new StraightProjectileEntity
            {
                isAlive = true,
                generation = generation,
                flight = request.projectile.flight,
                hit = request.projectile.hit,
                explosion = request.projectile.explosion,
                projectileView = request.projectileView,
                motion = new StraightProjectileMotionComponent { velocity = request.velocity },
                position = new PositionComponent { previous = request.position, current = request.position },
                rotation = new RotationComponent { previous = request.rotation, current = request.rotation },
                payload = new AttackPayloadComponent { faction = request.faction, damage = request.damage }
            };
        }

        // 칸 전체를 새 값으로 바꿔 이전 값이 남지 않게 한다.
        private static void Spawn(ref ArcProjectileEntity entity, in ArcProjectileSpawnRequest request)
        {
            int generation = entity.generation + 1;

            entity = new ArcProjectileEntity
            {
                isAlive = true,
                generation = generation,
                flight = request.projectile.flight,
                hit = request.projectile.hit,
                explosion = request.projectile.explosion,
                projectileView = request.projectileView,
                motion = new ArcProjectileMotionComponent { launchPosition = request.position, targetPosition = request.targetPosition },
                position = new PositionComponent { previous = request.position, current = request.position },
                rotation = new RotationComponent { previous = request.rotation, current = request.rotation },
                payload = new AttackPayloadComponent { faction = request.faction, damage = request.damage }
            };
        }
    }
}
