using TopView.Commands;
using TopView.Entities;
using UnityEngine;

namespace TopView.Systems
{
    // 플레이어의 접지, 중력, 이동 충돌, 조준 회전을 처리한다. 지형과는 Physics 질의로만 만난다.
    public static class PlayerMovementSystem
    {
        // 한 틱에 장애물을 따라 미끄러지는 최대 횟수
        private const int MAX_SLIDE_ITERATIONS = 3;
        // 이보다 짧은 남은 이동은 무시한다(유닛)
        private const float MIN_MOVE_DISTANCE = 0.00001f;
        // 조준점이 이보다 가까우면 돌지 않는다(유닛²)
        private const float MIN_AIM_DISTANCE_SQUARED = 0.01f;
        // 플레이어와 적의 중심이 이보다 가까우면 밀어낼 방향을 정할 수 없어 건너뛴다(유닛)
        private const float MIN_BLOCK_DISTANCE = 0.00001f;

        /// <summary>
        /// 직전 위치와 회전을 남기고, 접지를 판정해 위아래 속도를 갱신한 뒤 장애물을 따라 이동한다. 적·보스 쪽으로 파고들지 않게 막고, 회전이 잠겨 있지 않으면 조준점을 바라본다.
        /// </summary>
        /// <param name="players">플레이어 저장소</param>
        /// <param name="command">플레이어 입력</param>
        /// <param name="enemies">적 저장소. 플레이어를 막는다</param>
        /// <param name="bosses">보스 저장소. 플레이어를 막는다</param>
        public static void Tick(PlayerStorage players, in PlayerCommand command, EnemyStorage enemies, BossStorage bosses)
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
                BlockByEnemies(ref player, enemies, bosses);

                if (!player.isRotationLocked && command.hasAimPoint)
                {
                    Vector3 aimDirection = command.aimPoint - player.currentPosition;
                    aimDirection.y = 0f;
                    if (aimDirection.sqrMagnitude > MIN_AIM_DISTANCE_SQUARED)
                    {
                        player.currentRotation = Quaternion.LookRotation(aimDirection);
                    }
                }
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

        // 이번 틱에 적·보스의 피격 원 쪽으로 가까워졌다면 원 밖(XZ)으로 되돌린다. 적이 다가와 겹친 경우는 밀어내지 않는다.
        private static void BlockByEnemies(ref PlayerEntity player, EnemyStorage enemies, BossStorage bosses)
        {
            for (int enemyIndex = 0; enemyIndex < enemies.entities.Length; enemyIndex++)
            {
                ref readonly EnemyEntity enemy = ref enemies.entities[enemyIndex];
                if (enemy.isAlive)
                {
                    BlockByCircle(ref player, enemy.currentPosition, player.movement.capsuleRadius + enemy.stats.hitRadius);
                }
            }

            for (int bossIndex = 0; bossIndex < bosses.entities.Length; bossIndex++)
            {
                ref readonly BossEntity boss = ref bosses.entities[bossIndex];
                if (boss.isAlive)
                {
                    BlockByCircle(ref player, boss.currentPosition, player.movement.capsuleRadius + boss.stats.hitRadius);
                }
            }
        }

        private static void BlockByCircle(ref PlayerEntity player, Vector3 center, float minimumDistance)
        {
            Vector3 offset = player.currentPosition - center;
            offset.y = 0f;
            float distance = offset.magnitude;
            if (distance >= minimumDistance || distance < MIN_BLOCK_DISTANCE)
            {
                return;
            }

            Vector3 previousOffset = player.previousPosition - center;
            previousOffset.y = 0f;
            if (distance >= previousOffset.magnitude)
            {
                return;
            }

            Vector3 blockedOffset = offset / distance * minimumDistance;
            player.currentPosition = new Vector3(center.x + blockedOffset.x, player.currentPosition.y, center.z + blockedOffset.z);
        }
    }
}
