using System.Collections.Generic;
using TopView.Components;
using TopView.Entities;
using UnityEngine;

namespace TopView.Systems
{
    // 투사체를 움직인다. Straight는 수명이 다하면 사라지고, Grenade는 환경에 닿으면 멈춘다. 판정과 폭발은 ProjectileHitSystem이 한다.
    public static class ProjectileSystem
    {
        /// <summary>
        /// 투사체마다 시간을 1틱 늘린 뒤 움직인다.
        /// </summary>
        /// <param name="masks">칸별 컴포넌트 표시</param>
        /// <param name="projectiles">투사체 값</param>
        /// <param name="motions">투사체 이동 상태</param>
        /// <param name="positions">위치</param>
        /// <param name="freeProjectileIndices">투사체 구간의 빈 칸 번호 목록</param>
        public static void Tick(ComponentMask[] masks, ProjectileComponent[] projectiles, ProjectileMotionComponent[] motions, PositionComponent[] positions, List<int> freeProjectileIndices)
        {
            float deltaTime = Time.fixedDeltaTime;

            for (int index = 0; index < masks.Length; index++)
            {
                if ((masks[index] & ComponentMask.ProjectileArchetype) != ComponentMask.ProjectileArchetype)
                {
                    continue;
                }

                ref readonly ProjectileComponent projectile = ref projectiles[index];
                ref ProjectileMotionComponent motion = ref motions[index];
                ref PositionComponent position = ref positions[index];

                motion.elapsedTicks++;

                if (projectile.kind == ProjectileKind.Straight)
                {
                    position.current += motion.velocity * deltaTime;
                    if (motion.elapsedTicks >= projectile.lifetimeTicks)
                    {
                        SpawnSystem.ReleaseSlot(masks, freeProjectileIndices, index);
                    }
                    continue;
                }

                if (!motion.isResting)
                {
                    MoveGrenade(in projectile, ref motion, ref position, deltaTime);
                }
            }
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
