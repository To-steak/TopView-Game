using TopView.Components;
using TopView.Entities;
using UnityEngine;

namespace TopView.Systems
{
    // 판정 System이 저장소 종류를 몰라도 되도록, 공격을 맞을 수 있는 플레이어·적·보스를 한 목록에 모은다.
    public static class HitTargetSystem
    {
        // 공격을 맞을 수 있는 엔티티가 가지는 컴포넌트
        private const ComponentMask HIT_TARGET_QUERY = ComponentMask.Position | ComponentMask.Health | ComponentMask.EnemyStats;

        /// <summary>
        /// 살아 있으며 사망 상태가 아닌 플레이어와 체력이 남은 공유 저장소 엔티티로 목록을 처음부터 다시 채운다. 이동이 끝난 뒤, 판정 전에 부른다.
        /// 피해는 틱 맨 앞에서 반영되고 상태 System이 체력 0을 보고 사망 상태로 바꾸므로, 여기서는 체력이 0이면 사망 상태다.
        /// </summary>
        /// <param name="players">플레이어 저장소</param>
        /// <param name="masks">공유 저장소의 칸별 컴포넌트 표시</param>
        /// <param name="generations">공유 저장소의 칸이 채워진 횟수</param>
        /// <param name="positions">공유 저장소의 위치</param>
        /// <param name="healths">공유 저장소의 체력</param>
        /// <param name="enemyStats">공유 저장소의 피격 반지름 값</param>
        /// <param name="hitTargets">피격 대상 목록. 길이는 플레이어·적·보스 칸 수의 합이다</param>
        /// <param name="hitTargetCount">목록에서 이번 틱에 채운 대상 수</param>
        public static void Tick(PlayerStorage players, ComponentMask[] masks, int[] generations, PositionComponent[] positions, HealthComponent[] healths, EnemyStatsComponent[] enemyStats, HitTargetComponent[] hitTargets, ref int hitTargetCount)
        {
            hitTargetCount = 0;

            for (int playerIndex = 0; playerIndex < players.entities.Length; playerIndex++)
            {
                ref readonly PlayerEntity player = ref players.entities[playerIndex];
                if (player.isAlive && player.actionState.state != PlayerState.Die)
                {
                    // 점프로 적 투사체를 피할 수 있도록 플레이어만 캡슐 높이로 맞는다
                    PlayerMovementSystem.GetCapsulePoints(in player, player.position.current, out Vector3 top, out Vector3 bottom);
                    float radius = player.movement.capsuleRadius;
                    Add(hitTargets, ref hitTargetCount, EntityKind.Player, playerIndex, player.generation, Faction.Player, player.position.current, radius, bottom.y - radius, top.y + radius);
                }
            }

            for (int index = 0; index < masks.Length; index++)
            {
                if ((masks[index] & HIT_TARGET_QUERY) == HIT_TARGET_QUERY && healths[index].current > 0f)
                {
                    Add(hitTargets, ref hitTargetCount, EntityKind.Shared, index, generations[index], Faction.Enemy, positions[index].current, enemyStats[index].hitRadius, float.NegativeInfinity, float.PositiveInfinity);
                }
            }
        }

        private static void Add(HitTargetComponent[] hitTargets, ref int hitTargetCount, EntityKind kind, int index, int generation, Faction faction, Vector3 position, float radius, float bottomHeight, float topHeight)
        {
            hitTargets[hitTargetCount] = new HitTargetComponent
            {
                target = new EntityReference
                {
                    kind = kind,
                    identifier = new EntityIdentifier { index = index, generation = generation }
                },
                faction = faction,
                position = position,
                radius = radius,
                bottomHeight = bottomHeight,
                topHeight = topHeight
            };
            hitTargetCount++;
        }
    }
}
