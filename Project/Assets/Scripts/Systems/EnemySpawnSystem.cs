using System;
using System.Collections.Generic;
using TopView.Components;
using TopView.Entities;
using TopView.Requests;
using UnityEngine;
using UnityEngine.AI;

namespace TopView.Systems
{
    // 적 생성 요청을 받아 EnemyStorage의 그 종류 구간 빈 칸에 엔티티를 채운다. 틱 마지막에 부른다.
    public static class EnemySpawnSystem
    {
        /// <summary>
        /// 적 생성 요청을 모두 처리한다. 종류 번호가 범위 밖이거나 빈 칸이 없으면 요청을 버린다.
        /// </summary>
        /// <param name="enemies">적 저장소</param>
        /// <param name="enemySpawnRequests">적 생성 요청 큐</param>
        public static void Tick(EnemyStorage enemies, Queue<EnemySpawnRequest> enemySpawnRequests)
        {
            int droppedCount = 0;
            int missingNavMeshCount = 0;
            Vector3 missingNavMeshPosition = Vector3.zero;
            while (enemySpawnRequests.Count > 0)
            {
                EnemySpawnRequest request = enemySpawnRequests.Dequeue();
                if (request.enemyKindIndex < 0 || request.enemyKindIndex >= enemies.freeIndicesByKind.Length
                    || !SpawnSystem.TryTakeFreeIndex(enemies.freeIndicesByKind[request.enemyKindIndex], out int index))
                {
                    droppedCount++;
                    continue;
                }

                if (!Spawn(ref enemies.entities[index], in request))
                {
                    missingNavMeshCount++;
                    missingNavMeshPosition = request.position;
                }
            }

            if (droppedCount > 0)
            {
                Debug.LogWarning($"EnemySpawnSystem >> 적 종류 번호가 범위 밖이거나 빈 칸이 없어 적 생성 요청 {droppedCount}개를 버렸다.");
            }
            if (missingNavMeshCount > 0)
            {
                Debug.LogWarning($"EnemySpawnSystem >> 생성 위치 근처에 NavMesh가 없어 그 위치에 그대로 만든 적 {missingNavMeshCount}마리가 있다. 마지막 생성 위치: {missingNavMeshPosition}");
            }
        }

        // 생성 위치 근처에 NavMesh가 없으면 그 위치에 그대로 만들고 false를 돌려준다.
        private static bool Spawn(ref EnemyEntity slot, in EnemySpawnRequest request)
        {
            EnemyAttackComponent[] attacks = SpawnSystem.ReuseArray(slot.attacks, request.attacks);
            Vector3[] pathCorners = slot.pathCorners;
            if (pathCorners == null || pathCorners.Length != request.navigation.maxPathCorners)
            {
                pathCorners = new Vector3[request.navigation.maxPathCorners];
            }
            Array.Clear(pathCorners, 0, pathCorners.Length);

            Vector3 position = request.position;
            bool hasNavMesh = NavMesh.SamplePosition(position, out NavMeshHit hit, request.navigation.navMeshSampleDistance, NavMesh.AllAreas);
            if (hasNavMesh)
            {
                position = hit.position;
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
            return hasNavMesh;
        }
    }
}
