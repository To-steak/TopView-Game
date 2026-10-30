using TopView.Components;
using TopView.Entities;
using UnityEngine;

namespace TopView.Systems
{
    // 투사체를 움직인다. Straight는 수명이 다하면 사라지고, Grenade는 환경에 닿으면 멈춘다. 판정과 폭발은 HitSystem이 한다.
    public static class ProjectileSystem
    {
        /// <summary>
        /// 살아 있는 투사체마다 직전 위치를 남기고 시간을 1틱 늘린 뒤 움직인다.
        /// </summary>
        /// <param name="projectiles">투사체 저장소</param>
        public static void Tick(ProjectileStorage projectiles)
        {
            float deltaTime = Time.fixedDeltaTime;

            for (int projectileIndex = 0; projectileIndex < projectiles.entities.Length; projectileIndex++)
            {
                ref ProjectileEntity projectile = ref projectiles.entities[projectileIndex];
                if (!projectile.isAlive)
                {
                    continue;
                }

                projectile.previousPosition = projectile.currentPosition;
                projectile.elapsedTicks++;

                if (projectile.projectile.kind == ProjectileKind.Straight)
                {
                    projectile.currentPosition += projectile.velocity * deltaTime;
                    if (projectile.elapsedTicks >= projectile.projectile.lifetimeTicks)
                    {
                        Despawn(projectiles, projectileIndex);
                    }
                    continue;
                }

                if (!projectile.isResting)
                {
                    MoveGrenade(ref projectile, deltaTime);
                }
            }
        }

        /// <summary>
        /// 투사체를 즉시 없애고 칸을 반납한다.
        /// </summary>
        /// <param name="projectiles">투사체 저장소</param>
        /// <param name="projectileIndex">없앨 칸 번호</param>
        public static void Despawn(ProjectileStorage projectiles, int projectileIndex)
        {
            projectiles.entities[projectileIndex].isAlive = false;
            projectiles.freeIndices.Add(projectileIndex);
        }

        // 중력을 받아 움직이다가 환경 레이어에 닿으면 그 자리에서 멈춘다.
        private static void MoveGrenade(ref ProjectileEntity projectile, float deltaTime)
        {
            projectile.velocity.y += projectile.projectile.gravity * deltaTime;
            Vector3 displacement = projectile.velocity * deltaTime;
            float distance = displacement.magnitude;
            if (distance <= 0f)
            {
                return;
            }

            Vector3 direction = displacement / distance;
            if (Physics.SphereCast(projectile.currentPosition, projectile.projectile.radius, direction, out RaycastHit hit, distance, projectile.projectile.collisionLayer, QueryTriggerInteraction.Ignore))
            {
                projectile.currentPosition += direction * hit.distance;
                projectile.velocity = Vector3.zero;
                projectile.isResting = true;
                return;
            }

            projectile.currentPosition += displacement;
        }
    }
}
