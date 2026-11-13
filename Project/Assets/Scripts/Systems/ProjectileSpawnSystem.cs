using System.Collections.Generic;
using TopView.Components;
using TopView.Entities;
using TopView.Requests;
using TopView.Utilities;
using UnityEngine;

namespace TopView.Systems
{
    // 투사체 생성 요청을 받아 저장소 투사체 구간의 빈 칸을 채운다. 틱 마지막에 부른다.
    public static class ProjectileSpawnSystem
    {
        /// <summary>
        /// 직선 투사체 생성 요청을 모두 처리한다. 빈 칸이 없으면 요청을 버린다.
        /// </summary>
        /// <param name="storage">엔티티 저장소. 직선 투사체 구간의 칸을 Position, Rotation, StraightFlight, StraightMotion, ProjectileHit, Explosion, ProjectileView, AttackPayload로 채운다</param>
        /// <param name="spawnRequests">직선 투사체 생성 요청 큐</param>
        public static void Tick(EntityStorage storage, Queue<StraightProjectileSpawnRequest> spawnRequests)
        {
            int droppedCount = 0;
            while (spawnRequests.Count > 0)
            {
                StraightProjectileSpawnRequest request = spawnRequests.Dequeue();
                if (!SpawnUtility.TryTakeFreeIndex(storage.freeStraightProjectileIndices, out int index))
                {
                    droppedCount++;
                    continue;
                }

                Spawn(ref storage.entities[index], in request);
            }

            if (droppedCount > 0)
            {
                Debug.LogWarning($"ProjectileSpawnSystem >> 직선 투사체 구간에 빈 칸이 없어 생성 요청 {droppedCount}개를 버렸다.");
            }
        }

        /// <summary>
        /// 포물선 투사체 생성 요청을 모두 처리한다. 빈 칸이 없거나 flightTicks가 0 이하면 요청을 버린다.
        /// </summary>
        /// <param name="storage">엔티티 저장소. 포물선 투사체 구간의 칸을 Position, Rotation, ArcFlight, ArcMotion, ProjectileHit, Explosion, ProjectileView, AttackPayload로 채운다</param>
        /// <param name="spawnRequests">포물선 투사체 생성 요청 큐</param>
        public static void Tick(EntityStorage storage, Queue<ArcProjectileSpawnRequest> spawnRequests)
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
                if (!SpawnUtility.TryTakeFreeIndex(storage.freeArcProjectileIndices, out int index))
                {
                    droppedCount++;
                    continue;
                }

                Spawn(ref storage.entities[index], in request);
            }

            if (invalidFlightCount > 0)
            {
                Debug.LogWarning($"ProjectileSpawnSystem >> flightTicks가 0 이하인 포물선 투사체 생성 요청 {invalidFlightCount}개를 버렸다. flightTicks는 0보다 커야 한다.");
            }
            if (droppedCount > 0)
            {
                Debug.LogWarning($"ProjectileSpawnSystem >> 포물선 투사체 구간에 빈 칸이 없어 생성 요청 {droppedCount}개를 버렸다.");
            }
        }

        // 칸 전체를 새 값으로 바꿔 이전 값이 남지 않게 한다.
        private static void Spawn(ref Entity entity, in StraightProjectileSpawnRequest request)
        {
            int generation = entity.generation + 1;

            entity = new Entity
            {
                mask = ComponentMask.Position | ComponentMask.Rotation | ComponentMask.StraightFlight | ComponentMask.StraightMotion
                    | ComponentMask.ProjectileHit | ComponentMask.Explosion | ComponentMask.ProjectileView | ComponentMask.AttackPayload,
                generation = generation,
                straightFlight = request.projectile.flight,
                projectileHit = request.projectile.hit,
                explosion = request.projectile.explosion,
                projectileView = request.projectileView,
                straightMotion = new StraightProjectileMotionComponent { velocity = request.velocity },
                position = new PositionComponent { previous = request.position, current = request.position },
                rotation = new RotationComponent { previous = request.rotation, current = request.rotation },
                payload = new AttackPayloadComponent { faction = request.faction, damage = request.damage }
            };
        }

        // 칸 전체를 새 값으로 바꿔 이전 값이 남지 않게 한다.
        private static void Spawn(ref Entity entity, in ArcProjectileSpawnRequest request)
        {
            int generation = entity.generation + 1;

            entity = new Entity
            {
                mask = ComponentMask.Position | ComponentMask.Rotation | ComponentMask.ArcFlight | ComponentMask.ArcMotion
                    | ComponentMask.ProjectileHit | ComponentMask.Explosion | ComponentMask.ProjectileView | ComponentMask.AttackPayload,
                generation = generation,
                arcFlight = request.projectile.flight,
                projectileHit = request.projectile.hit,
                explosion = request.projectile.explosion,
                projectileView = request.projectileView,
                arcMotion = new ArcProjectileMotionComponent { launchPosition = request.position, targetPosition = request.targetPosition },
                position = new PositionComponent { previous = request.position, current = request.position },
                rotation = new RotationComponent { previous = request.rotation, current = request.rotation },
                payload = new AttackPayloadComponent { faction = request.faction, damage = request.damage }
            };
        }
    }
}
