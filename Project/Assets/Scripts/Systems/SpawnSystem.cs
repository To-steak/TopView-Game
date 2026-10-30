using System;
using System.Collections.Generic;
using TopView.Components;
using TopView.Entities;
using TopView.Requests;
using UnityEngine;
using UnityEngine.AI;

namespace TopView.Systems
{
    // 생성 요청을 받아 Storage의 빈 칸에 엔티티를 채운다. 틱 마지막에 부른다.
    public static class SpawnSystem
    {

        /// <summary>
        /// 플레이어·적·보스·투사체 생성 요청을 모두 처리한다. 빈 칸이 없으면 요청을 버린다.
        /// </summary>
        /// <param name="players">플레이어 저장소</param>
        /// <param name="playerSpawnRequests">플레이어 생성 요청 큐</param>
        /// <param name="enemies">적 저장소</param>
        /// <param name="enemySpawnRequests">적 생성 요청 큐</param>
        /// <param name="bosses">보스 저장소</param>
        /// <param name="bossSpawnRequests">보스 생성 요청 큐</param>
        /// <param name="projectiles">투사체 저장소</param>
        /// <param name="projectileSpawnRequests">투사체 생성 요청 큐</param>
        public static void Tick(PlayerStorage players, Queue<PlayerSpawnRequest> playerSpawnRequests, EnemyStorage enemies, Queue<EnemySpawnRequest> enemySpawnRequests, BossStorage bosses, Queue<BossSpawnRequest> bossSpawnRequests, ProjectileStorage projectiles, Queue<ProjectileSpawnRequest> projectileSpawnRequests)
        {
            int droppedPlayerCount = 0;
            while (playerSpawnRequests.Count > 0)
            {
                PlayerSpawnRequest request = playerSpawnRequests.Dequeue();
                if (!TryTakeFreeIndex(players.freeIndices, out int index))
                {
                    droppedPlayerCount++;
                    continue;
                }

                SpawnPlayer(ref players.entities[index], in request);
            }

            int droppedEnemyCount = 0;
            while (enemySpawnRequests.Count > 0)
            {
                EnemySpawnRequest request = enemySpawnRequests.Dequeue();
                if (request.enemyKindIndex < 0 || request.enemyKindIndex >= enemies.freeIndicesByKind.Length
                    || !TryTakeFreeIndex(enemies.freeIndicesByKind[request.enemyKindIndex], out int index))
                {
                    droppedEnemyCount++;
                    continue;
                }

                SpawnEnemy(ref enemies.entities[index], in request);
            }

            bool hasPlayer = EnemyBehaviorSystem.TryGetPlayerTarget(players, out EntityIdentifier _, out Vector3 playerPosition, out float _);
            int droppedBossCount = 0;
            while (bossSpawnRequests.Count > 0)
            {
                BossSpawnRequest request = bossSpawnRequests.Dequeue();
                if (!TryTakeFreeIndex(bosses.freeIndices, out int index))
                {
                    droppedBossCount++;
                    continue;
                }

                SpawnBoss(ref bosses.entities[index], in request, hasPlayer, playerPosition);
            }

            int droppedProjectileCount = 0;
            while (projectileSpawnRequests.Count > 0)
            {
                ProjectileSpawnRequest request = projectileSpawnRequests.Dequeue();
                if (!TryTakeFreeIndex(projectiles.freeIndices, out int index))
                {
                    droppedProjectileCount++;
                    continue;
                }

                ref ProjectileEntity slot = ref projectiles.entities[index];
                slot = new ProjectileEntity
                {
                    isAlive = true,
                    generation = slot.generation + 1,
                    projectile = request.projectile,
                    isTargetingPlayer = request.isTargetingPlayer,
                    damage = request.damage,
                    previousPosition = request.position,
                    currentPosition = request.position,
                    rotation = request.rotation,
                    velocity = request.velocity
                };
            }

            if (droppedPlayerCount > 0)
            {
                Debug.LogWarning($"SpawnSystem >> 플레이어 저장소에 빈 칸이 없어 생성 요청 {droppedPlayerCount}개를 버렸다.");
            }
            if (droppedEnemyCount > 0)
            {
                Debug.LogWarning($"SpawnSystem >> 적 종류 번호가 범위 밖이거나 빈 칸이 없어 적 생성 요청 {droppedEnemyCount}개를 버렸다.");
            }
            if (droppedBossCount > 0)
            {
                Debug.LogWarning($"SpawnSystem >> 보스 저장소에 빈 칸이 없어 생성 요청 {droppedBossCount}개를 버렸다.");
            }
            if (droppedProjectileCount > 0)
            {
                Debug.LogWarning($"SpawnSystem >> 투사체 저장소에 빈 칸이 없어 생성 요청 {droppedProjectileCount}개를 버렸다.");
            }
        }

        private static bool TryTakeFreeIndex(List<int> freeIndices, out int index)
        {
            if (freeIndices.Count == 0)
            {
                index = -1;
                return false;
            }

            index = freeIndices[freeIndices.Count - 1];
            freeIndices.RemoveAt(freeIndices.Count - 1);
            return true;
        }

        private static void SpawnPlayer(ref PlayerEntity slot, in PlayerSpawnRequest request)
        {
            WeaponComponent[] weapons = ReuseArray(slot.weapons, request.weapons);
            EntityIdentifier[] meleeHitEnemies = slot.meleeHitEnemies;
            if (meleeHitEnemies == null || meleeHitEnemies.Length != request.meleeHitCapacity)
            {
                meleeHitEnemies = new EntityIdentifier[request.meleeHitCapacity];
            }
            Array.Clear(meleeHitEnemies, 0, meleeHitEnemies.Length);

            slot = new PlayerEntity
            {
                isAlive = true,
                generation = slot.generation + 1,
                movement = request.movement,
                vitals = request.vitals,
                action = request.action,
                previousPosition = request.position,
                currentPosition = request.position,
                previousRotation = request.rotation,
                currentRotation = request.rotation,
                state = PlayerState.Idle,
                currentSpeed = request.movement.walkSpeed,
                health = request.vitals.maxHealth,
                mana = request.vitals.maxMana,
                manaRecoveryElapsedTicks = request.vitals.manaRecoveryDelayTicks,
                weapons = weapons,
                meleeHitEnemies = meleeHitEnemies,
                currentWeaponIndex = 0,
                pendingWeaponIndex = 0
            };
        }

        private static void SpawnEnemy(ref EnemyEntity slot, in EnemySpawnRequest request)
        {
            EnemyAttackComponent[] attacks = ReuseArray(slot.attacks, request.attacks);
            Vector3[] pathCorners = slot.pathCorners;
            if (pathCorners == null || pathCorners.Length != request.navigation.maxPathCorners)
            {
                pathCorners = new Vector3[request.navigation.maxPathCorners];
            }
            Array.Clear(pathCorners, 0, pathCorners.Length);

            Vector3 position = request.position;
            if (NavMesh.SamplePosition(position, out NavMeshHit hit, request.navigation.navMeshSampleDistance, NavMesh.AllAreas))
            {
                position = hit.position;
            }
            else
            {
                Debug.LogWarning($"SpawnSystem >> 적 생성 위치 {request.position} 근처에 NavMesh가 없어 그 위치에 그대로 만들었다.");
            }

            float maxHealth = request.stats.basicMaxHealth + request.level * request.stats.levelIncrement;
            slot = new EnemyEntity
            {
                isAlive = true,
                generation = slot.generation + 1,
                kind = request.enemyKindIndex,
                stats = request.stats,
                behavior = request.behavior,
                navigation = request.navigation,
                attacks = attacks,
                level = request.level,
                maxHealth = maxHealth,
                health = maxHealth,
                attackDamage = request.stats.basicAttackDamage + request.level * request.stats.levelIncrement,
                previousPosition = position,
                currentPosition = position,
                previousRotation = request.rotation,
                currentRotation = request.rotation,
                state = EnemyState.Idle,
                pathCorners = pathCorners
            };
        }

        // 보스는 도발 상태로 생기며, 도발을 시작할 때처럼 플레이어 쪽(XZ)을 바로 바라본다.
        private static void SpawnBoss(ref BossEntity slot, in BossSpawnRequest request, bool hasPlayer, Vector3 playerPosition)
        {
            EnemyAttackComponent[] attacks = ReuseArray(slot.attacks, request.attacks);

            Quaternion rotation = Quaternion.identity;
            Vector3 toPlayer = playerPosition - request.position;
            toPlayer.y = 0f;
            if (hasPlayer && toPlayer.sqrMagnitude > 0f)
            {
                rotation = Quaternion.LookRotation(toPlayer);
            }

            float maxHealth = request.stats.basicMaxHealth + request.level * request.stats.levelIncrement;
            slot = new BossEntity
            {
                isAlive = true,
                generation = slot.generation + 1,
                stats = request.stats,
                behavior = request.behavior,
                attacks = attacks,
                level = request.level,
                maxHealth = maxHealth,
                health = maxHealth,
                attackDamage = request.stats.basicAttackDamage + request.level * request.stats.levelIncrement,
                previousPosition = request.position,
                currentPosition = request.position,
                previousRotation = rotation,
                currentRotation = rotation,
                state = BossState.Taunt
            };
        }

        // 칸이 쓰던 배열을 길이가 같으면 다시 쓰고, 비운 뒤 요청의 내용을 복사한다.
        private static T[] ReuseArray<T>(T[] slotArray, T[] requestArray)
        {
            T[] array = slotArray;
            if (array == null || array.Length != requestArray.Length)
            {
                array = new T[requestArray.Length];
            }
            Array.Clear(array, 0, array.Length);
            Array.Copy(requestArray, array, requestArray.Length);
            return array;
        }
    }
}
