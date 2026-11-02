using TopView.Components;
using TopView.Entities;
using UnityEngine;

namespace TopView.Systems
{
    // 공격을 맞을 수 있는 엔티티를 방향별 목록에 모은다. 플레이어 목록은 적의 공격이, 적 목록은 플레이어의 공격이 읽는다.
    public static class HitTargetSystem
    {
        // 캡슐 높이로 맞는 엔티티가 가지는 컴포넌트
        private const ComponentMask CAPSULE_TARGET_QUERY = ComponentMask.Position | ComponentMask.Rotation | ComponentMask.Health | ComponentMask.PlayerMovement;
        // 높이를 따지지 않고 맞는 엔티티가 가지는 컴포넌트
        private const ComponentMask CIRCLE_TARGET_QUERY = ComponentMask.Position | ComponentMask.Health | ComponentMask.EnemyStats;

        /// <summary>
        /// 체력이 남은 엔티티로 두 목록을 처음부터 다시 채운다. 이동이 끝난 뒤, 판정 전에 부른다.
        /// 피해는 틱 맨 앞에서 반영되고 상태 System이 체력 0을 보고 사망 상태로 바꾸므로, 여기서는 체력이 0이면 사망 상태다.
        /// </summary>
        /// <param name="masks">칸별 컴포넌트 표시</param>
        /// <param name="generations">칸이 채워진 횟수</param>
        /// <param name="positions">위치</param>
        /// <param name="rotations">회전</param>
        /// <param name="healths">체력</param>
        /// <param name="movements">플레이어 이동 값. 캡슐 크기를 읽는다</param>
        /// <param name="enemyStats">피격 반지름 값</param>
        /// <param name="playerHitTargets">플레이어 피격 대상 목록. 길이는 플레이어 칸 수다</param>
        /// <param name="playerHitTargetCount">플레이어 피격 대상 목록에서 이번 틱에 채운 대상 수</param>
        /// <param name="enemyHitTargets">적 피격 대상 목록. 길이는 적 칸 수다</param>
        /// <param name="enemyHitTargetCount">적 피격 대상 목록에서 이번 틱에 채운 대상 수</param>
        public static void Tick(ComponentMask[] masks, int[] generations, PositionComponent[] positions, RotationComponent[] rotations, HealthComponent[] healths, PlayerMovementComponent[] movements, EnemyStatsComponent[] enemyStats, HitTargetComponent[] playerHitTargets, ref int playerHitTargetCount, HitTargetComponent[] enemyHitTargets, ref int enemyHitTargetCount)
        {
            playerHitTargetCount = 0;
            enemyHitTargetCount = 0;

            for (int index = 0; index < masks.Length; index++)
            {
                if ((masks[index] & CAPSULE_TARGET_QUERY) == CAPSULE_TARGET_QUERY && healths[index].current > 0f)
                {
                    // 점프로 적 투사체를 피할 수 있도록 플레이어만 캡슐 높이로 맞는다
                    PlayerMovementSystem.GetCapsulePoints(in movements[index], rotations[index].current, positions[index].current, out Vector3 top, out Vector3 bottom);
                    float radius = movements[index].capsuleRadius;
                    Add(playerHitTargets, ref playerHitTargetCount, index, generations[index], positions[index].current, radius, bottom.y - radius, top.y + radius);
                }

                if ((masks[index] & CIRCLE_TARGET_QUERY) == CIRCLE_TARGET_QUERY && healths[index].current > 0f)
                {
                    Add(enemyHitTargets, ref enemyHitTargetCount, index, generations[index], positions[index].current, enemyStats[index].hitRadius, float.NegativeInfinity, float.PositiveInfinity);
                }
            }
        }

        private static void Add(HitTargetComponent[] hitTargets, ref int hitTargetCount, int index, int generation, Vector3 position, float radius, float bottomHeight, float topHeight)
        {
            hitTargets[hitTargetCount] = new HitTargetComponent
            {
                target = new EntityIdentifier { index = index, generation = generation },
                position = position,
                radius = radius,
                bottomHeight = bottomHeight,
                topHeight = topHeight
            };
            hitTargetCount++;
        }
    }
}
