using TopView.Components;
using TopView.Entities;
using UnityEngine;

namespace TopView.Systems
{
    // 플레이어가 적의 피격 원 쪽으로 파고들지 않게 막는다.
    public static class PlayerBlockSystem
    {
        // 플레이어와 적의 중심이 이보다 가까우면 밀어낼 방향을 정할 수 없어 건너뛴다(유닛)
        private const float MIN_BLOCK_DISTANCE = 0.00001f;

        /// <summary>
        /// 이번 틱에 적의 피격 원 쪽으로 가까워졌다면 원 밖(XZ)으로 되돌린다. 적이 다가와 겹친 경우는 밀어내지 않는다.
        /// </summary>
        /// <param name="players">플레이어 저장소</param>
        /// <param name="enemies">적 저장소</param>
        public static void Tick(PlayerStorage players, EnemyStorage enemies)
        {
            for (int index = 0; index < players.entities.Length; index++)
            {
                ref PlayerEntity entity = ref players.entities[index];
                if (entity.isAlive)
                {
                    BlockByEnemies(ref entity.position, entity.movement.capsuleRadius, enemies.entities);
                }
            }
        }

        private static void BlockByEnemies(ref PositionComponent position, float capsuleRadius, EnemyEntity[] enemies)
        {
            for (int index = 0; index < enemies.Length; index++)
            {
                ref readonly EnemyEntity enemy = ref enemies[index];
                if (enemy.isAlive)
                {
                    BlockByCircle(ref position, enemy.position.current, capsuleRadius + enemy.body.radius);
                }
            }
        }

        private static void BlockByCircle(ref PositionComponent position, Vector3 center, float minimumDistance)
        {
            Vector3 offset = position.current - center;
            offset.y = 0f;
            float distance = offset.magnitude;
            if (distance >= minimumDistance || distance < MIN_BLOCK_DISTANCE)
            {
                return;
            }

            Vector3 previousOffset = position.previous - center;
            previousOffset.y = 0f;
            if (distance * distance >= previousOffset.sqrMagnitude)
            {
                return;
            }

            Vector3 blockedOffset = offset / distance * minimumDistance;
            position.current = new Vector3(center.x + blockedOffset.x, position.current.y, center.z + blockedOffset.z);
        }
    }
}
