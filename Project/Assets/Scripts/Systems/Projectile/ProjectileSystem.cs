using TopView.Components;
using TopView.Entities;
using TopView.Utilities;
using UnityEngine;

namespace TopView.Systems
{
    // 투사체를 움직인다. 둘 다 환경에 닿으면 멈추고, 포물선 투사체는 착탄점에 도착해도 멈춘다. 판정과 폭발, 직선 투사체의 소멸(명중, 벽, 수명)은 ProjectileHitSystem이 한다.
    public static class ProjectileSystem
    {
        // 직선 투사체가 가진 컴포넌트
        private const ComponentMask STRAIGHT_MASK = ComponentMask.Position | ComponentMask.StraightFlight | ComponentMask.StraightMotion | ComponentMask.ProjectileHit;
        // 포물선 투사체가 가진 컴포넌트
        private const ComponentMask ARC_MASK = ComponentMask.Position | ComponentMask.ArcFlight | ComponentMask.ArcMotion | ComponentMask.ProjectileHit;

        /// <summary>
        /// 직선 투사체마다 시간을 1틱 늘린 뒤 곧게 움직인다. 포물선 투사체마다 시간을 1틱 늘린 뒤, 멈추지 않았으면 포물선을 따라 움직인다.
        /// </summary>
        /// <param name="storage">엔티티 저장소. 직선 투사체(Position, StraightFlight, StraightMotion, ProjectileHit)와 포물선 투사체(Position, ArcFlight, ArcMotion, ProjectileHit)를 처리하고, 이동 상태와 position을 고친다</param>
        public static void Tick(EntityStorage storage)
        {
            float deltaTime = Time.fixedDeltaTime;

            for (int index = 0; index < storage.entities.Length; index++)
            {
                ref Entity entity = ref storage.entities[index];
                if ((entity.mask & STRAIGHT_MASK) == STRAIGHT_MASK)
                {
                    entity.straightMotion.elapsedTicks++;
                    MoveStraight(in entity.projectileHit, ref entity.straightMotion, ref entity.position, deltaTime);
                }
                else if ((entity.mask & ARC_MASK) == ARC_MASK)
                {
                    entity.arcMotion.elapsedTicks++;
                    if (!entity.arcMotion.isResting)
                    {
                        MoveArc(in entity.arcFlight, in entity.projectileHit, ref entity.arcMotion, ref entity.position);
                    }
                }
            }
        }

        // 곧게 움직이다가 환경 레이어에 닿으면 닿은 자리까지만 옮기고 멈춘다.
        // 소멸은 ProjectileHitSystem이 이번 틱의 명중 판정을 마친 뒤 요청한다. 벽 앞에 있던 대상이 같은 틱에 맞을 수 있게 하기 위해서다.
        private static void MoveStraight(in ProjectileHitComponent hit, ref StraightProjectileMotionComponent motion, ref PositionComponent position, float deltaTime)
        {
            Vector3 displacement = motion.velocity * deltaTime;
            float distance = displacement.magnitude;
            if (distance <= 0f)
            {
                return;
            }

            Vector3 direction = displacement / distance;
            if (Physics.SphereCast(position.current, hit.radius, direction, out RaycastHit raycastHit, distance, hit.collisionLayer, QueryTriggerInteraction.Ignore))
            {
                position.current += direction * raycastHit.distance;
                motion.velocity = Vector3.zero;
                motion.isResting = true;
                return;
            }

            position.current += displacement;
        }

        // 지난 시간으로 포물선 위의 다음 위치를 구해 옮긴다. 가는 길에 환경 레이어에 닿으면 닿은 자리에서, 아니면 착탄점에 도착하면 멈춘다.
        // 위치를 누적하지 않고 매 틱 공식으로 구하므로 오차가 쌓이지 않는다. 투척 미리보기도 같은 계산을 쓴다.
        private static void MoveArc(in ArcFlightComponent flight, in ProjectileHitComponent hit, ref ArcProjectileMotionComponent motion, ref PositionComponent position)
        {
            float progress = ThrowableUtility.GetArcProgress(motion.elapsedTicks, flight.flightTicks);
            Vector3 nextPosition = ThrowableUtility.GetArcPosition(motion.launchPosition, motion.targetPosition, flight.arcHeight, progress);

            if (ThrowableUtility.TrySweep(in hit, position.current, nextPosition, out Vector3 stopPosition))
            {
                position.current = stopPosition;
                motion.isResting = true;
                return;
            }

            position.current = nextPosition;
            motion.isResting = progress >= 1f;
        }
    }
}
