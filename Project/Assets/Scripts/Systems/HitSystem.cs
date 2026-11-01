using System.Collections.Generic;
using TopView.Components;
using TopView.Entities;
using TopView.Records;
using TopView.Requests;
using UnityEngine;

namespace TopView.Systems
{
    // 투사체 명중, 수류탄 폭발, 플레이어 근접 공격을 XZ 평면의 원으로 판정해 피해 요청을 넣는다.
    public static class HitSystem
    {
        /// <summary>
        /// Straight 투사체는 이번 틱의 이동 선분에서 가장 먼저 닿은 대상 하나에, 시간이 된 Grenade는 폭발 반경 안의 적·보스 모두에게 피해 요청을 넣고 사라진다.
        /// 근접 판정이 켜진 플레이어는 판정 구 안에서 이번 공격에 아직 맞지 않은 적·보스에게 피해 요청을 넣는다.
        /// </summary>
        /// <param name="projectiles">투사체 저장소</param>
        /// <param name="players">플레이어 저장소</param>
        /// <param name="enemies">적 저장소</param>
        /// <param name="bosses">보스 저장소</param>
        /// <param name="damageRequests">피해 요청 큐</param>
        /// <param name="hitRecords">명중·폭발 기록</param>
        public static void Tick(ProjectileStorage projectiles, PlayerStorage players, EnemyStorage enemies, BossStorage bosses, Queue<DamageRequest> damageRequests, List<ProjectileHitRecord> hitRecords)
        {
            bool hasPlayer = TargetingSystem.TryGetPlayerTarget(players, out EntityIdentifier playerIdentifier, out Vector3 playerPosition, out float playerRadius);

            for (int projectileIndex = 0; projectileIndex < projectiles.entities.Length; projectileIndex++)
            {
                ref ProjectileEntity projectile = ref projectiles.entities[projectileIndex];
                if (!projectile.isAlive)
                {
                    continue;
                }

                if (projectile.projectile.kind == ProjectileKind.Grenade)
                {
                    if (projectile.elapsedTicks >= projectile.projectile.fuseTicks)
                    {
                        Explode(in projectile, enemies, bosses, damageRequests, hitRecords);
                        ProjectileSystem.Despawn(projectiles, projectileIndex);
                    }
                    continue;
                }

                bool isHit = projectile.faction == Faction.Enemy
                    ? HitPlayer(in projectile, hasPlayer, in playerIdentifier, playerPosition, playerRadius, damageRequests, hitRecords)
                    : HitNearestEnemy(in projectile, enemies, bosses, damageRequests, hitRecords);
                if (isHit)
                {
                    ProjectileSystem.Despawn(projectiles, projectileIndex);
                }
            }

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

        private static bool HitPlayer(in ProjectileEntity projectile, bool hasPlayer, in EntityIdentifier playerIdentifier, Vector3 playerPosition, float playerRadius, Queue<DamageRequest> damageRequests, List<ProjectileHitRecord> hitRecords)
        {
            if (!hasPlayer || !TryHitSegment(projectile.previousPosition, projectile.currentPosition, playerPosition, projectile.projectile.radius + playerRadius, out float hitFraction))
            {
                return false;
            }

            RequestDamage(damageRequests, EntityKind.Player, in playerIdentifier, projectile.damage);
            AddHitRecord(in projectile, hitFraction, hitRecords);
            return true;
        }

        // 이동 선분에서 가장 먼저 닿는 적이나 보스 하나에게 피해를 준다.
        private static bool HitNearestEnemy(in ProjectileEntity projectile, EnemyStorage enemies, BossStorage bosses, Queue<DamageRequest> damageRequests, List<ProjectileHitRecord> hitRecords)
        {
            float nearestFraction = float.MaxValue;
            int nearestEnemyIndex = -1;
            int nearestBossIndex = -1;

            for (int enemyIndex = 0; enemyIndex < enemies.entities.Length; enemyIndex++)
            {
                ref readonly EnemyEntity enemy = ref enemies.entities[enemyIndex];
                if (enemy.isAlive
                    && TryHitSegment(projectile.previousPosition, projectile.currentPosition, enemy.currentPosition, projectile.projectile.radius + enemy.stats.hitRadius, out float hitFraction)
                    && hitFraction < nearestFraction)
                {
                    nearestFraction = hitFraction;
                    nearestEnemyIndex = enemyIndex;
                }
            }

            for (int bossIndex = 0; bossIndex < bosses.entities.Length; bossIndex++)
            {
                ref readonly BossEntity boss = ref bosses.entities[bossIndex];
                if (boss.isAlive
                    && TryHitSegment(projectile.previousPosition, projectile.currentPosition, boss.currentPosition, projectile.projectile.radius + boss.stats.hitRadius, out float hitFraction)
                    && hitFraction < nearestFraction)
                {
                    nearestFraction = hitFraction;
                    nearestEnemyIndex = -1;
                    nearestBossIndex = bossIndex;
                }
            }

            if (nearestBossIndex >= 0)
            {
                EntityIdentifier target = new EntityIdentifier { index = nearestBossIndex, generation = bosses.entities[nearestBossIndex].generation };
                RequestDamage(damageRequests, EntityKind.Boss, in target, projectile.damage);
            }
            else if (nearestEnemyIndex >= 0)
            {
                EntityIdentifier target = new EntityIdentifier { index = nearestEnemyIndex, generation = enemies.entities[nearestEnemyIndex].generation };
                RequestDamage(damageRequests, EntityKind.Enemy, in target, projectile.damage);
            }
            else
            {
                return false;
            }

            AddHitRecord(in projectile, nearestFraction, hitRecords);
            return true;
        }

        private static void Explode(in ProjectileEntity projectile, EnemyStorage enemies, BossStorage bosses, Queue<DamageRequest> damageRequests, List<ProjectileHitRecord> hitRecords)
        {
            for (int enemyIndex = 0; enemyIndex < enemies.entities.Length; enemyIndex++)
            {
                ref readonly EnemyEntity enemy = ref enemies.entities[enemyIndex];
                if (enemy.isAlive && TargetingSystem.IsWithin(projectile.currentPosition, enemy.currentPosition, projectile.projectile.explosionRadius + enemy.stats.hitRadius))
                {
                    EntityIdentifier target = new EntityIdentifier { index = enemyIndex, generation = enemy.generation };
                    RequestDamage(damageRequests, EntityKind.Enemy, in target, projectile.damage);
                }
            }

            for (int bossIndex = 0; bossIndex < bosses.entities.Length; bossIndex++)
            {
                ref readonly BossEntity boss = ref bosses.entities[bossIndex];
                if (boss.isAlive && TargetingSystem.IsWithin(projectile.currentPosition, boss.currentPosition, projectile.projectile.explosionRadius + boss.stats.hitRadius))
                {
                    EntityIdentifier target = new EntityIdentifier { index = bossIndex, generation = boss.generation };
                    RequestDamage(damageRequests, EntityKind.Boss, in target, projectile.damage);
                }
            }

            hitRecords.Add(new ProjectileHitRecord { position = projectile.currentPosition, hitEffectIndex = projectile.projectile.hitEffectIndex });
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

        // 선분 from→to(XZ)가 center를 중심으로 한 반지름 radius의 원에 닿는지. 닿으면 선분에서 가장 가까운 점의 비율(0~1)을 돌려준다.
        private static bool TryHitSegment(Vector3 from, Vector3 to, Vector3 center, float radius, out float hitFraction)
        {
            Vector2 start = new Vector2(from.x, from.z);
            Vector2 segment = new Vector2(to.x, to.z) - start;
            Vector2 toCenter = new Vector2(center.x, center.z) - start;
            float segmentLengthSquared = segment.sqrMagnitude;

            hitFraction = segmentLengthSquared > 0f ? Mathf.Clamp01(Vector2.Dot(toCenter, segment) / segmentLengthSquared) : 0f;
            Vector2 closestOffset = toCenter - segment * hitFraction;
            return closestOffset.sqrMagnitude <= radius * radius;
        }

        private static void AddHitRecord(in ProjectileEntity projectile, float hitFraction, List<ProjectileHitRecord> hitRecords)
        {
            hitRecords.Add(new ProjectileHitRecord
            {
                position = Vector3.Lerp(projectile.previousPosition, projectile.currentPosition, hitFraction),
                hitEffectIndex = projectile.projectile.hitEffectIndex
            });
        }
    }
}
