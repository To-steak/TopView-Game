using TopView.Components;
using TopView.Entities;
using UnityEngine;

namespace TopView.Systems
{
    // 플레이어의 접지, 중력, 이동 충돌을 처리한다. 지형과는 Physics 질의로만 만난다.
    public static class PlayerMovementSystem
    {
        // 한 틱에 장애물을 따라 미끄러지는 최대 횟수
        private const int MAX_SLIDE_ITERATIONS = 3;
        // 이보다 짧은 남은 이동은 무시한다(유닛)
        private const float MIN_MOVE_DISTANCE = 0.00001f;

        // 이 System이 움직이는 엔티티가 가지는 컴포넌트
        private const ComponentMask MOVE_QUERY = ComponentMask.Position | ComponentMask.Rotation | ComponentMask.PlayerMovement | ComponentMask.PlayerMotion;

        /// <summary>
        /// 접지를 판정해 위아래 속도를 갱신한 뒤 장애물을 따라 이동한다.
        /// </summary>
        /// <param name="masks">칸별 컴포넌트 표시</param>
        /// <param name="positions">위치</param>
        /// <param name="rotations">회전</param>
        /// <param name="movements">플레이어 이동 값</param>
        /// <param name="motions">플레이어 이동 상태</param>
        public static void Tick(ComponentMask[] masks, PositionComponent[] positions, RotationComponent[] rotations, PlayerMovementComponent[] movements, PlayerMotionComponent[] motions)
        {
            float deltaTime = Time.fixedDeltaTime;

            for (int index = 0; index < masks.Length; index++)
            {
                if ((masks[index] & MOVE_QUERY) != MOVE_QUERY)
                {
                    continue;
                }

                ref PositionComponent position = ref positions[index];
                ref readonly PlayerMovementComponent movement = ref movements[index];
                ref PlayerMotionComponent motion = ref motions[index];

                motion.isGrounded = Physics.CheckSphere(position.current + movement.groundCheckOffset, movement.groundCheckRadius, movement.groundLayer);
                if (motion.isGrounded && motion.verticalVelocity < 0f)
                {
                    motion.verticalVelocity = movement.groundPressure;
                }
                motion.verticalVelocity += movement.gravity * deltaTime;

                Vector3 displacement = motion.moveDirection * motion.currentSpeed * deltaTime;
                displacement.y = motion.verticalVelocity * deltaTime;
                position.current = Slide(in movement, rotations[index].current, position.current, displacement);
            }
        }

        /// <summary>
        /// 플레이어 이동 충돌 캡슐의 위아래 구 중심을 구한다.
        /// </summary>
        /// <param name="movement">플레이어 이동 값</param>
        /// <param name="rotation">플레이어 회전(월드)</param>
        /// <param name="position">캡슐을 놓을 플레이어 위치</param>
        /// <param name="top">위쪽 구의 중심</param>
        /// <param name="bottom">아래쪽 구의 중심</param>
        public static void GetCapsulePoints(in PlayerMovementComponent movement, Quaternion rotation, Vector3 position, out Vector3 top, out Vector3 bottom)
        {
            Vector3 center = position + rotation * movement.capsuleCenter;
            float halfLine = Mathf.Max(movement.capsuleHeight * 0.5f - movement.capsuleRadius, 0f);
            top = center + Vector3.up * halfLine;
            bottom = center - Vector3.up * halfLine;
        }

        // 캡슐을 displacement만큼 옮기다가 장애물에 닿으면 닿은 면을 따라 남은 이동을 미끄러뜨린다.
        private static Vector3 Slide(in PlayerMovementComponent movement, Quaternion rotation, Vector3 position, Vector3 displacement)
        {
            for (int iteration = 0; iteration < MAX_SLIDE_ITERATIONS; iteration++)
            {
                float distance = displacement.magnitude;
                if (distance < MIN_MOVE_DISTANCE)
                {
                    break;
                }

                Vector3 direction = displacement / distance;
                GetCapsulePoints(in movement, rotation, position, out Vector3 top, out Vector3 bottom);
                if (!Physics.CapsuleCast(top, bottom, movement.capsuleRadius, direction, out RaycastHit hit, distance + movement.skinWidth, movement.collisionLayer, QueryTriggerInteraction.Ignore))
                {
                    position += displacement;
                    break;
                }

                float travel = Mathf.Max(hit.distance - movement.skinWidth, 0f);
                position += direction * travel;

                Vector3 remaining = direction * (distance - travel);
                displacement = remaining - Vector3.Dot(remaining, hit.normal) * hit.normal;
            }

            return position;
        }
    }
}
