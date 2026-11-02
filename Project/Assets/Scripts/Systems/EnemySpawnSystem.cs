using System;
using System.Collections.Generic;
using TopView.Components;
using TopView.Entities;
using TopView.Requests;
using UnityEngine;
using UnityEngine.AI;

namespace TopView.Systems
{
    // 적 생성 요청을 받아 공유 저장소의 그 종류 구간 빈 칸에 컴포넌트를 채운다. 틱 마지막에 부른다.
    public static class EnemySpawnSystem
    {
        /// <summary>
        /// 적 생성 요청을 모두 처리한다. 종류 번호가 범위 밖이거나 빈 칸이 없으면 요청을 버린다.
        /// </summary>
        /// <param name="entities">공유 저장소</param>
        /// <param name="enemySpawnRequests">적 생성 요청 큐</param>
        /// <param name="navMeshSampleDistance">생성 위치를 NavMesh 위로 맞출 때 찾는 최대 거리(유닛)</param>
        public static void Tick(EntityStorage entities, Queue<EnemySpawnRequest> enemySpawnRequests, float navMeshSampleDistance)
        {
            int droppedCount = 0;
            int missingNavMeshCount = 0;
            Vector3 missingNavMeshPosition = Vector3.zero;
            while (enemySpawnRequests.Count > 0)
            {
                EnemySpawnRequest request = enemySpawnRequests.Dequeue();
                if (request.enemyKindIndex < 0 || request.enemyKindIndex >= entities.freeEnemyIndicesByKind.Length
                    || !SpawnSystem.TryTakeFreeIndex(entities.freeEnemyIndicesByKind[request.enemyKindIndex], out int index))
                {
                    droppedCount++;
                    continue;
                }

                if (!Spawn(entities, index, in request, navMeshSampleDistance))
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
        private static bool Spawn(EntityStorage entities, int index, in EnemySpawnRequest request, float navMeshSampleDistance)
        {
            EnemyAttackComponent[] attacks = SpawnSystem.ReuseArray(entities.attackLists[index].attacks, request.attacks);
            Vector3[] pathCorners = entities.navigationAgents[index].pathCorners;
            Array.Clear(pathCorners, 0, pathCorners.Length);
            int[] attackCooldownRemainingTicks = entities.enemyActionStates[index].attackCooldownRemainingTicks;
            if (attackCooldownRemainingTicks == null || attackCooldownRemainingTicks.Length != attacks.Length)
            {
                attackCooldownRemainingTicks = new int[attacks.Length];
            }
            Array.Clear(attackCooldownRemainingTicks, 0, attackCooldownRemainingTicks.Length);

            Vector3 position = request.position;
            bool hasNavMesh = NavMesh.SamplePosition(position, out NavMeshHit hit, navMeshSampleDistance, NavMesh.AllAreas);
            if (hasNavMesh)
            {
                position = hit.position;
            }

            float maxHealth = request.stats.baseMaxHealth + request.level * request.stats.maxHealthIncrement;

            SpawnSystem.ClaimSlot(entities, index, ComponentMask.EnemyArchetype);
            entities.enemyKinds[index] = new EnemyKindComponent { kind = request.enemyKindIndex };
            entities.enemyStats[index] = request.stats;
            entities.enemyBehaviors[index] = request.behavior;
            entities.attackLists[index] = new AttackListComponent { attacks = attacks };
            entities.combats[index] = new CombatComponent
            {
                level = request.level,
                attackDamage = request.stats.baseAttackDamage + request.level * request.stats.attackDamageIncrement
            };
            entities.healths[index] = new HealthComponent { current = maxHealth, max = maxHealth };
            entities.positions[index] = new PositionComponent { previous = position, current = position };
            entities.rotations[index] = new RotationComponent { previous = request.rotation, current = request.rotation };
            entities.enemyActionStates[index] = new EnemyActionStateComponent
            {
                state = EnemyState.Idle,
                attackCooldownRemainingTicks = attackCooldownRemainingTicks
            };
            entities.navigationAgents[index] = new NavigationAgentComponent
            {
                maxSpeed = request.stats.baseSpeed + request.level * request.stats.speedIncrement,
                pathCorners = pathCorners
            };
            return hasNavMesh;
        }
    }
}
