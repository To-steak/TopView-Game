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

        /// <summary>
        /// 직전 위치와 회전을 남기고, 접지를 판정해 위아래 속도를 갱신한 뒤 장애물을 따라 이동한다.
        /// </summary>
        /// <param name="players">플레이어 저장소</param>
        public static void Tick(PlayerStorage players)
        {
            float deltaTime = Time.fixedDeltaTime;

            for (int playerIndex = 0; playerIndex < players.entities.Length; playerIndex++)
            {
                ref PlayerEntity player = ref players.entities[playerIndex];
                if (!player.isAlive)
                {
                    continue;
                }

                player.previousPosition = player.currentPosition;
                player.previousRotation = player.currentRotation;

                player.isGrounded = Physics.CheckSphere(player.currentPosition + player.movement.groundCheckOffset, player.movement.groundCheckRadius, player.movement.groundLayer);
                if (player.isGrounded && player.verticalVelocity < 0f)
                {
                    player.verticalVelocity = player.movement.groundPressure;
                }
                player.verticalVelocity += player.movement.gravity * deltaTime;

                Vector3 displacement = player.moveDirection * player.currentSpeed * deltaTime;
                displacement.y = player.verticalVelocity * deltaTime;
                player.currentPosition = Slide(in player, player.currentPosition, displacement);
            }
        }

        /// <summary>
        /// 플레이어 이동 충돌 캡슐의 위아래 구 중심을 구한다.
        /// </summary>
        /// <param name="player">플레이어</param>
        /// <param name="position">캡슐을 놓을 플레이어 위치</param>
        /// <param name="top">위쪽 구의 중심</param>
        /// <param name="bottom">아래쪽 구의 중심</param>
        public static void GetCapsulePoints(in PlayerEntity player, Vector3 position, out Vector3 top, out Vector3 bottom)
        {
            Vector3 center = position + player.currentRotation * player.movement.capsuleCenter;
            float halfLine = Mathf.Max(player.movement.capsuleHeight * 0.5f - player.movement.capsuleRadius, 0f);
            top = center + Vector3.up * halfLine;
            bottom = center - Vector3.up * halfLine;
        }

        // 캡슐을 displacement만큼 옮기다가 장애물에 닿으면 닿은 면을 따라 남은 이동을 미끄러뜨린다.
        private static Vector3 Slide(in PlayerEntity player, Vector3 position, Vector3 displacement)
        {
            for (int iteration = 0; iteration < MAX_SLIDE_ITERATIONS; iteration++)
            {
                float distance = displacement.magnitude;
                if (distance < MIN_MOVE_DISTANCE)
                {
                    break;
                }

                Vector3 direction = displacement / distance;
                GetCapsulePoints(in player, position, out Vector3 top, out Vector3 bottom);
                if (!Physics.CapsuleCast(top, bottom, player.movement.capsuleRadius, direction, out RaycastHit hit, distance + player.movement.skinWidth, player.movement.collisionLayer, QueryTriggerInteraction.Ignore))
                {
                    position += displacement;
                    break;
                }

                float travel = Mathf.Max(hit.distance - player.movement.skinWidth, 0f);
                position += direction * travel;

                Vector3 remaining = direction * (distance - travel);
                displacement = remaining - Vector3.Dot(remaining, hit.normal) * hit.normal;
            }

            return position;
        }
    }
}
