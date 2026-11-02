using TopView.Components;
using TopView.Entities;
using UnityEngine;

namespace TopView.Systems
{
    // 플레이어가 적·보스의 피격 원 쪽으로 파고들지 않게 막는다.
    public static class PlayerBlockSystem
    {
        // 플레이어와 적의 중심이 이보다 가까우면 밀어낼 방향을 정할 수 없어 건너뛴다(유닛)
        private const float MIN_BLOCK_DISTANCE = 0.00001f;
        // 막히는 엔티티가 가지는 컴포넌트
        private const ComponentMask BLOCKED_QUERY = ComponentMask.Position | ComponentMask.PlayerMovement;
        // 플레이어를 막는 엔티티가 가지는 컴포넌트
        private const ComponentMask BLOCKER_QUERY = ComponentMask.Position | ComponentMask.EnemyStats;

        /// <summary>
        /// 이번 틱에 적·보스의 피격 원 쪽으로 가까워졌다면 원 밖(XZ)으로 되돌린다. 적이 다가와 겹친 경우는 밀어내지 않는다.
        /// </summary>
        /// <param name="masks">칸별 컴포넌트 표시</param>
        /// <param name="positions">위치</param>
        /// <param name="movements">플레이어 이동 값</param>
        /// <param name="enemyStats">피격 반지름 값</param>
        public static void Tick(ComponentMask[] masks, PositionComponent[] positions, PlayerMovementComponent[] movements, EnemyStatsComponent[] enemyStats)
        {
            for (int index = 0; index < masks.Length; index++)
            {
                if ((masks[index] & BLOCKED_QUERY) == BLOCKED_QUERY)
                {
                    BlockByEnemies(ref positions[index], movements[index].capsuleRadius, masks, positions, enemyStats);
                }
            }
        }

        private static void BlockByEnemies(ref PositionComponent position, float capsuleRadius, ComponentMask[] masks, PositionComponent[] positions, EnemyStatsComponent[] enemyStats)
        {
            for (int index = 0; index < masks.Length; index++)
            {
                if ((masks[index] & BLOCKER_QUERY) == BLOCKER_QUERY)
                {
                    BlockByCircle(ref position, positions[index].current, capsuleRadius + enemyStats[index].hitRadius);
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
