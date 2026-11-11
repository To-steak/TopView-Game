using TopView.Components;
using TopView.Entities;
using TopView.Utilities;
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

        /// <summary>
        /// 땅에 파묻혔으면 땅 위로 올리고, 접지를 판정해 위아래 속도를 갱신한 뒤 장애물을 따라 이동한다.
        /// </summary>
        /// <param name="players">플레이어 저장소</param>
        public static void Tick(PlayerStorage players)
        {
            float deltaTime = Time.fixedDeltaTime;

            for (int index = 0; index < players.entities.Length; index++)
            {
                ref PlayerEntity entity = ref players.entities[index];
                if (entity.isAlive)
                {
                    Move(ref entity.position, ref entity.motion, in entity.movement, entity.rotation.current, deltaTime);
                }
            }
        }

        private static void Move(ref PositionComponent position, ref PlayerMotionComponent motion, in PlayerMovementComponent movement, Quaternion rotation, float deltaTime)
        {
            position.current = PushOutOfGround(in movement, rotation, position.current);
            motion.isGrounded = Physics.CheckSphere(position.current + movement.groundCheckOffset, movement.groundCheckRadius, movement.groundLayer);
            if (motion.isGrounded && motion.verticalVelocity < 0f)
            {
                motion.verticalVelocity = movement.groundPressure;
            }
            motion.verticalVelocity += movement.gravity * deltaTime;

            Vector3 displacement = motion.moveDirection * motion.currentSpeed * deltaTime;
            displacement.y = motion.verticalVelocity * deltaTime;
            position.current = Slide(in movement, rotation, position.current, displacement);
        }

        // 캡슐이 땅에 파묻혀 있으면 캡슐 높이만큼 위에서 아래로 캡슐을 내려 처음 닿는 땅 위로 올린다.
        // CapsuleCast는 시작할 때 이미 겹친 Collider를 보지 못해, 파묻힌 채로 움직이면 그 땅을 통과해 아래로 빠진다
        private static Vector3 PushOutOfGround(in PlayerMovementComponent movement, Quaternion rotation, Vector3 position)
        {
            PlayerMovementUtility.GetCapsulePoints(in movement, rotation, position, out Vector3 top, out Vector3 bottom);
            if (!Physics.CheckCapsule(top, bottom, movement.capsuleRadius, movement.groundLayer, QueryTriggerInteraction.Ignore))
            {
                return position;
            }

            float liftDistance = movement.capsuleHeight;
            Vector3 lifted = position + Vector3.up * liftDistance;
            PlayerMovementUtility.GetCapsulePoints(in movement, rotation, lifted, out top, out bottom);
            if (!Physics.CapsuleCast(top, bottom, movement.capsuleRadius, Vector3.down, out RaycastHit hit, liftDistance, movement.groundLayer, QueryTriggerInteraction.Ignore))
            {
                Debug.LogWarning("PlayerMovementSystem >> 플레이어 캡슐이 땅에 파묻혔지만 캡슐 높이 위에서 내려올 땅을 찾지 못해 그대로 두었다.");
                return position;
            }

            return lifted + Vector3.down * Mathf.Max(hit.distance - movement.skinWidth, 0f);
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
                PlayerMovementUtility.GetCapsulePoints(in movement, rotation, position, out Vector3 top, out Vector3 bottom);
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
