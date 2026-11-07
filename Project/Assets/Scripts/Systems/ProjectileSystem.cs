using System.Collections.Generic;
using TopView.Components;
using TopView.Entities;
using TopView.Requests;
using TopView.Utilities;
using UnityEngine;

namespace TopView.Systems
{
    // 투사체를 움직인다. Straight는 수명이 다하면 소멸을 요청하고, 둘 다 환경에 닿으면 멈춘다. Grenade는 착탄점에 도착해도 멈춘다. 판정과 폭발, 벽에 막힌 Straight의 소멸은 ProjectileHitSystem이 한다.
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
                MoveStraight(in projectile, ref motion, ref position, deltaTime);
                return motion.elapsedTicks >= projectile.lifetimeTicks;
            }

            if (!motion.isResting)
            {
                MoveGrenade(in projectile, ref motion, ref position);
            }
            return false;
        }

        // 곧게 움직이다가 환경 레이어에 닿으면 닿은 자리까지만 옮기고 멈춘다.
        // 소멸은 ProjectileHitSystem이 이번 틱의 명중 판정을 마친 뒤 요청한다. 벽 앞에 있던 대상이 같은 틱에 맞을 수 있게 하기 위해서다.
        private static void MoveStraight(in ProjectileComponent projectile, ref ProjectileMotionComponent motion, ref PositionComponent position, float deltaTime)
        {
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

        // 지난 시간으로 포물선 위의 다음 위치를 구해 옮긴다. 가는 길에 환경 레이어에 닿으면 닿은 자리에서, 아니면 착탄점에 도착하면 멈춘다.
        // 위치를 누적하지 않고 매 틱 공식으로 구하므로 오차가 쌓이지 않는다. 투척 미리보기도 같은 계산을 쓴다.
        private static void MoveGrenade(in ProjectileComponent projectile, ref ProjectileMotionComponent motion, ref PositionComponent position)
        {
            float progress = ThrowableUtility.GetArcProgress(motion.elapsedTicks, projectile.flightTicks);
            Vector3 nextPosition = ThrowableUtility.GetArcPosition(motion.launchPosition, motion.targetPosition, projectile.arcHeight, progress);

            if (TrySweep(in projectile, position.current, nextPosition, out Vector3 stopPosition))
            {
                position.current = stopPosition;
                motion.isResting = true;
                return;
            }

            position.current = nextPosition;
            motion.isResting = progress >= 1f;
        }

        // from에서 to로 한 틱 움직일 때 환경 레이어에 닿는지 투사체 반지름의 구로 검사한다. 닿으면 닿은 자리, 아니면 to를 stopPosition에 넣는다.
        // 투척 미리보기와 결과가 같도록 PlayerThrowPreviewSystem.TrySweep과 같은 질의를 한다. 한쪽을 고치면 다른 쪽도 고친다.
        private static bool TrySweep(in ProjectileComponent projectile, Vector3 from, Vector3 to, out Vector3 stopPosition)
        {
            stopPosition = to;
            Vector3 displacement = to - from;
            float distance = displacement.magnitude;
            if (distance <= 0f)
            {
                return false;
            }

            Vector3 direction = displacement / distance;
            if (Physics.SphereCast(from, projectile.radius, direction, out RaycastHit hit, distance, projectile.collisionLayer, QueryTriggerInteraction.Ignore))
            {
                stopPosition = from + direction * hit.distance;
                return true;
            }

            return false;
        }
    }
}
