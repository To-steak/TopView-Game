using TopView.Components;
using TopView.Entities;
using TopView.Utilities;
using UnityEngine;

namespace TopView.Systems
{
    // 공격을 맞을 수 있는 엔티티를 방향별 목록에 모은다. 플레이어 목록은 적의 공격이, 적 목록은 플레이어의 공격이 읽는다.
    public static class HitTargetSystem
    {
        /// <summary>
        /// 체력이 남은 엔티티로 두 목록을 처음부터 다시 채운다. 이동이 끝난 뒤, 판정 전에 부른다.
        /// 피해는 틱 맨 앞에서 반영되고 상태 System이 체력 0을 보고 사망 상태로 바꾸므로, 여기서는 체력이 0이면 사망 상태다.
        /// </summary>
        /// <param name="players">플레이어 저장소</param>
        /// <param name="enemies">적 저장소</param>
        /// <param name="playerHitTargets">플레이어 피격 대상 목록. 길이는 플레이어 칸 수다</param>
        /// <param name="playerHitTargetCount">플레이어 피격 대상 목록에서 이번 틱에 채운 대상 수</param>
        /// <param name="enemyHitTargets">적 피격 대상 목록. 길이는 적 칸 수다</param>
        /// <param name="enemyHitTargetCount">적 피격 대상 목록에서 이번 틱에 채운 대상 수</param>
        public static void Tick(PlayerStorage players, EnemyStorage enemies, HitTargetComponent[] playerHitTargets, ref int playerHitTargetCount, HitTargetComponent[] enemyHitTargets, ref int enemyHitTargetCount)
        {
            playerHitTargetCount = 0;
            enemyHitTargetCount = 0;

            for (int index = 0; index < players.entities.Length; index++)
            {
                ref readonly PlayerEntity player = ref players.entities[index];
                if (player.isAlive && player.health.current > 0f)
                {
                    // 점프로 적 투사체를 피할 수 있도록 플레이어만 캡슐 높이로 맞는다
                    PlayerMovementUtility.GetCapsulePoints(in player.movement, player.rotation.current, player.position.current, out Vector3 top, out Vector3 bottom);
                    float radius = player.movement.capsuleRadius;
                    Add(playerHitTargets, ref playerHitTargetCount, index, player.generation, player.position.current, radius, bottom.y - radius, top.y + radius);
                }
            }

            for (int index = 0; index < enemies.entities.Length; index++)
            {
                ref readonly EnemyEntity enemy = ref enemies.entities[index];
                if (enemy.isAlive && enemy.health.current > 0f)
                {
                    Add(enemyHitTargets, ref enemyHitTargetCount, index, enemy.generation, enemy.position.current, enemy.body.radius, float.NegativeInfinity, float.PositiveInfinity);
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
