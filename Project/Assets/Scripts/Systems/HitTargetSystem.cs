using TopView.Components;
using TopView.Entities;
using UnityEngine;

namespace TopView.Systems
{
    // 판정 System이 저장소 종류를 몰라도 되도록, 공격을 맞을 수 있는 플레이어·적·보스를 한 목록에 모은다.
    public static class HitTargetSystem
    {
        /// <summary>
        /// 살아 있으며 사망 상태가 아닌 플레이어, 적, 보스 순으로 목록을 처음부터 다시 채운다. 이동이 끝난 뒤, 판정 전에 부른다.
        /// </summary>
        /// <param name="players">플레이어 저장소</param>
        /// <param name="enemies">적 저장소</param>
        /// <param name="bosses">보스 저장소</param>
        /// <param name="hitTargets">피격 대상 목록. 길이는 세 저장소 칸 수의 합이다</param>
        /// <param name="hitTargetCount">목록에서 이번 틱에 채운 대상 수</param>
        public static void Tick(PlayerStorage players, EnemyStorage enemies, BossStorage bosses, HitTargetComponent[] hitTargets, ref int hitTargetCount)
        {
            hitTargetCount = 0;

            for (int playerIndex = 0; playerIndex < players.entities.Length; playerIndex++)
            {
                ref readonly PlayerEntity player = ref players.entities[playerIndex];
                if (player.isAlive && player.state != PlayerState.Die)
                {
                    Add(hitTargets, ref hitTargetCount, EntityKind.Player, playerIndex, player.generation, Faction.Player, player.currentPosition, player.movement.capsuleRadius);
                }
            }

            for (int enemyIndex = 0; enemyIndex < enemies.entities.Length; enemyIndex++)
            {
                ref readonly EnemyEntity enemy = ref enemies.entities[enemyIndex];
                if (enemy.isAlive && enemy.state != EnemyState.Die)
                {
                    Add(hitTargets, ref hitTargetCount, EntityKind.Enemy, enemyIndex, enemy.generation, Faction.Enemy, enemy.currentPosition, enemy.stats.hitRadius);
                }
            }

            for (int bossIndex = 0; bossIndex < bosses.entities.Length; bossIndex++)
            {
                ref readonly BossEntity boss = ref bosses.entities[bossIndex];
                if (boss.isAlive && boss.state != BossState.Die)
                {
                    Add(hitTargets, ref hitTargetCount, EntityKind.Boss, bossIndex, boss.generation, Faction.Enemy, boss.currentPosition, boss.stats.hitRadius);
                }
            }
        }

        private static void Add(HitTargetComponent[] hitTargets, ref int hitTargetCount, EntityKind kind, int index, int generation, Faction faction, Vector3 position, float radius)
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
                radius = radius
            };
            hitTargetCount++;
        }
    }
}
