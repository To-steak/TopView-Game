using TopView.Entities;
using UnityEngine;

namespace TopView.Systems
{
    // 플레이어가 적·보스의 피격 원 쪽으로 파고들지 않게 막는다.
    public static class PlayerBlockSystem
    {
        // 플레이어와 적의 중심이 이보다 가까우면 밀어낼 방향을 정할 수 없어 건너뛴다(유닛)
        private const float MIN_BLOCK_DISTANCE = 0.00001f;

        /// <summary>
        /// 이번 틱에 적·보스의 피격 원 쪽으로 가까워졌다면 원 밖(XZ)으로 되돌린다. 적이 다가와 겹친 경우는 밀어내지 않는다.
        /// </summary>
        /// <param name="players">플레이어 저장소</param>
        /// <param name="enemies">적 저장소. 플레이어를 막는다</param>
        /// <param name="bosses">보스 저장소. 플레이어를 막는다</param>
        public static void Tick(PlayerStorage players, EnemyStorage enemies, BossStorage bosses)
        {
            for (int playerIndex = 0; playerIndex < players.entities.Length; playerIndex++)
            {
                ref PlayerEntity player = ref players.entities[playerIndex];
                if (!player.isAlive)
                {
                    continue;
                }

                BlockByEnemies(ref player, enemies, bosses);
            }
        }

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
            if (distance * distance >= previousOffset.sqrMagnitude)
            {
                return;
            }

            Vector3 blockedOffset = offset / distance * minimumDistance;
            player.currentPosition = new Vector3(center.x + blockedOffset.x, player.currentPosition.y, center.z + blockedOffset.z);
        }
    }
}
