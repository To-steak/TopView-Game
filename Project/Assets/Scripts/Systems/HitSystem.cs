using System.Collections.Generic;
using TopView.Components;
using TopView.Entities;
using TopView.Requests;
using UnityEngine;

namespace TopView.Systems
{
    // 플레이어 근접 공격을 XZ 평면의 원으로 판정해 피해 요청을 넣는다.
    public static class HitSystem
    {
        /// <summary>
        /// 근접 판정이 켜진 플레이어는 판정 구 안에서 이번 공격에 아직 맞지 않은 적·보스에게 피해 요청을 넣는다.
        /// </summary>
        /// <param name="players">플레이어 저장소</param>
        /// <param name="enemies">적 저장소</param>
        /// <param name="bosses">보스 저장소</param>
        /// <param name="damageRequests">피해 요청 큐</param>
        public static void Tick(PlayerStorage players, EnemyStorage enemies, BossStorage bosses, Queue<DamageRequest> damageRequests)
        {
            int droppedMeleeHitCount = 0;
            for (int playerIndex = 0; playerIndex < players.entities.Length; playerIndex++)
            {
                ref PlayerEntity player = ref players.entities[playerIndex];
                if (player.isAlive && player.isMeleeAttacking)
                {
                    droppedMeleeHitCount += HitByMelee(ref player, enemies, bosses, damageRequests);
                }
            }

            if (droppedMeleeHitCount > 0)
            {
                Debug.LogWarning($"HitSystem >> 근접 공격에 맞은 적을 기록할 칸이 모자라 적 {droppedMeleeHitCount}마리를 맞히지 않았다.");
            }
        }

        // 근접 판정 구 안에서 이번 공격에 아직 맞지 않은 적·보스를 한 틱에 최대 대상 수까지 맞힌다. 맞힌 적을 기록할 칸이 모자라 건너뛴 수를 돌려준다.
        private static int HitByMelee(ref PlayerEntity player, EnemyStorage enemies, BossStorage bosses, Queue<DamageRequest> damageRequests)
        {
            ref readonly WeaponComponent weapon = ref player.weapons[player.currentWeaponIndex];
            Vector3 center = player.currentPosition + player.currentRotation * Vector3.forward * weapon.meleeOffset;
            int hitCount = 0;
            int droppedCount = 0;

            for (int enemyIndex = 0; enemyIndex < enemies.entities.Length && hitCount < weapon.meleeMaxTargets; enemyIndex++)
            {
                ref readonly EnemyEntity enemy = ref enemies.entities[enemyIndex];
                if (!enemy.isAlive || !TargetingSystem.IsWithin(center, enemy.currentPosition, weapon.meleeRadius + enemy.stats.hitRadius))
                {
                    continue;
                }

                EntityIdentifier target = new EntityIdentifier { index = enemyIndex, generation = enemy.generation };
                if (IsAlreadyHit(in player, in target))
                {
                    continue;
                }

                if (player.meleeHitEnemyCount >= player.meleeHitEnemies.Length)
                {
                    droppedCount++;
                    continue;
                }

                player.meleeHitEnemies[player.meleeHitEnemyCount] = target;
                player.meleeHitEnemyCount++;
                RequestDamage(damageRequests, EntityKind.Enemy, in target, weapon.damage);
                hitCount++;
            }

            for (int bossIndex = 0; bossIndex < bosses.entities.Length && hitCount < weapon.meleeMaxTargets; bossIndex++)
            {
                ref readonly BossEntity boss = ref bosses.entities[bossIndex];
                if (!boss.isAlive || !TargetingSystem.IsWithin(center, boss.currentPosition, weapon.meleeRadius + boss.stats.hitRadius))
                {
                    continue;
                }

                EntityIdentifier target = new EntityIdentifier { index = bossIndex, generation = boss.generation };
                if (player.isMeleeBossHit && player.meleeHitBoss.index == target.index && player.meleeHitBoss.generation == target.generation)
                {
                    continue;
                }

                player.isMeleeBossHit = true;
                player.meleeHitBoss = target;
                RequestDamage(damageRequests, EntityKind.Boss, in target, weapon.damage);
                hitCount++;
            }

            return droppedCount;
        }

        private static void RequestDamage(Queue<DamageRequest> damageRequests, EntityKind kind, in EntityIdentifier identifier, float amount)
        {
            damageRequests.Enqueue(new DamageRequest
            {
                target = new EntityReference { kind = kind, identifier = identifier },
                amount = amount
            });
        }

        private static bool IsAlreadyHit(in PlayerEntity player, in EntityIdentifier target)
        {
            for (int hitIndex = 0; hitIndex < player.meleeHitEnemyCount; hitIndex++)
            {
                EntityIdentifier hit = player.meleeHitEnemies[hitIndex];
                if (hit.index == target.index && hit.generation == target.generation)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
