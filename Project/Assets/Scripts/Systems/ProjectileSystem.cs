using System.Collections.Generic;
using TopView.Components;
using TopView.Entities;
using TopView.Requests;
using UnityEngine;

namespace TopView.Systems
{
    // 투사체를 움직인다. Straight는 수명이 다하면 소멸을 요청하고, Grenade는 환경에 닿으면 멈춘다. 판정과 폭발은 ProjectileHitSystem이 한다.
    public static class ProjectileSystem
    {
        /// <summary>
        /// 투사체마다 시간을 1틱 늘린 뒤 움직인다.
        /// </summary>
        /// <param name="projectiles">투사체 저장소</param>
        /// <param name="despawnRequests">투사체 소멸 요청 큐</param>
        public static void Tick(ProjectileStorage projectiles, Queue<ProjectileDespawnRequest> despawnRequests)
        {
            float deltaTime = Time.fixedDeltaTime;

            for (int index = 0; index < projectiles.entities.Length; index++)
            {
                ref ProjectileEntity entity = ref projectiles.entities[index];
                if (entity.isAlive && Move(in entity.projectile, ref entity.motion, ref entity.position, deltaTime))
                {
                    despawnRequests.Enqueue(new ProjectileDespawnRequest { target = new EntityIdentifier { index = index, generation = entity.generation } });
                }
            }
        }

        // 시간을 1틱 늘리고 움직인다. 수명이 다한 Straight 투사체면 true를 돌려준다.
        private static bool Move(in ProjectileComponent projectile, ref ProjectileMotionComponent motion, ref PositionComponent position, float deltaTime)
        {
            motion.elapsedTicks++;

            if (projectile.kind == ProjectileKind.Straight)
            {
                position.current += motion.velocity * deltaTime;
                return motion.elapsedTicks >= projectile.lifetimeTicks;
            }

            if (!motion.isResting)
            {
                MoveGrenade(in projectile, ref motion, ref position, deltaTime);
            }
            return false;
        }

        // 중력을 받아 움직이다가 환경 레이어에 닿으면 그 자리에서 멈춘다.
        private static void MoveGrenade(in ProjectileComponent projectile, ref ProjectileMotionComponent motion, ref PositionComponent position, float deltaTime)
        {
            motion.velocity.y += projectile.gravity * deltaTime;
            Vector3 displacement = motion.velocity * deltaTime;
            float distance = displacement.magnitude;
            if (distance <= 0f)
            {
                return;
            }

            Vector3 direction = displacement / distance;
            if (Physics.SphereCast(position.current, projectile.radius, direction, out RaycastHit hit, distance, projectile.collisionLayer, QueryTriggerInteraction.Ignore))
            {
                position.current += direction * hit.distance;
                motion.velocity = Vector3.zero;
                motion.isResting = true;
                return;
            }

            position.current += displacement;
        }
    }
}
