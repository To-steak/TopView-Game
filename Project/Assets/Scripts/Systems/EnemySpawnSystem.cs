using System;
using System.Collections.Generic;
using TopView.Components;
using TopView.Entities;
using TopView.Requests;
using TopView.Utilities;
using UnityEngine;
using UnityEngine.AI;

namespace TopView.Systems
{
    // 적 생성 요청을 받아 저장소의 그 종류 구간 빈 칸을 채운다. 틱 마지막에 부른다.
    public static class EnemySpawnSystem
    {
        // 적이 가진 컴포넌트
        private const ComponentMask ENEMY_MASK = ComponentMask.Position | ComponentMask.Rotation | ComponentMask.EnemyKind | ComponentMask.Score | ComponentMask.Body
            | ComponentMask.NavigationMovement | ComponentMask.Death | ComponentMask.AttackList | ComponentMask.LootList | ComponentMask.Combat
            | ComponentMask.Health | ComponentMask.EnemyActionState | ComponentMask.NavigationAgent;

        /// <summary>
        /// 적 생성 요청을 모두 처리한다. 종류 번호가 범위 밖이거나 빈 칸이 없으면 요청을 버린다.
        /// </summary>
        /// <param name="storage">엔티티 저장소. 적 종류별 구간의 칸을 적 컴포넌트로 채운다</param>
        /// <param name="enemySpawnRequests">적 생성 요청 큐</param>
        /// <param name="navMeshSampleDistance">생성 위치를 NavMesh 위로 맞출 때 찾는 최대 거리(유닛)</param>
        public static void Tick(EntityStorage storage, Queue<EnemySpawnRequest> enemySpawnRequests, float navMeshSampleDistance)
        {
            int droppedCount = 0;
            int missingNavMeshCount = 0;
            Vector3 missingNavMeshPosition = Vector3.zero;
            while (enemySpawnRequests.Count > 0)
            {
                EnemySpawnRequest request = enemySpawnRequests.Dequeue();
                if (request.enemyKindIndex < 0 || request.enemyKindIndex >= storage.freeEnemyIndicesByKind.Length
                    || !SpawnUtility.TryTakeFreeIndex(storage.freeEnemyIndicesByKind[request.enemyKindIndex], out int index))
                {
                    droppedCount++;
                    continue;
                }

                if (!Spawn(ref storage.entities[index], in request, navMeshSampleDistance))
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

        // 칸 전체를 새 값으로 바꿔 이전 값이 남지 않게 한다. 공격 목록, 드롭 표, 경로, 공격별 대기 시간 배열만 비워서 다시 쓴다.
        // 생성 위치 근처에 NavMesh가 없으면 그 위치에 그대로 만들고 false를 돌려준다.
        private static bool Spawn(ref Entity entity, in EnemySpawnRequest request, float navMeshSampleDistance)
        {
            int generation = entity.generation + 1;
            EnemyAttackComponent[] attacks = SpawnUtility.ReuseArray(entity.attackList.attacks, request.attacks);
            EnemyLootComponent[] loots = SpawnUtility.ReuseArray(entity.lootList.loots, request.loots);
            Vector3[] pathCorners = entity.navigationAgent.pathCorners;
            Array.Clear(pathCorners, 0, pathCorners.Length);
            int[] attackCooldownRemainingTicks = entity.enemyActionState.attackCooldownRemainingTicks;
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

            float maxHealth = LevelScalingUtility.GetValue(in request.maxHealth, request.level);

            entity = new Entity
            {
                mask = ENEMY_MASK,
                generation = generation,
                enemyKind = new EnemyKindComponent { kind = request.enemyKindIndex },
                score = request.score,
                body = request.body,
                navigationMovement = request.movement,
                death = request.death,
                attackList = new AttackListComponent { attacks = attacks },
                lootList = new LootListComponent { loots = loots },
                combat = new CombatComponent
                {
                    level = request.level,
                    attackDamage = LevelScalingUtility.GetValue(in request.attackDamage, request.level)
                },
                health = new HealthComponent { current = maxHealth, max = maxHealth },
                position = new PositionComponent { previous = position, current = position },
                rotation = new RotationComponent { previous = request.rotation, current = request.rotation },
                enemyActionState = new EnemyActionStateComponent
                {
                    state = EnemyState.Idle,
                    attackCooldownRemainingTicks = attackCooldownRemainingTicks
                },
                navigationAgent = new NavigationAgentComponent
                {
                    maxSpeed = LevelScalingUtility.GetValue(in request.speed, request.level),
                    pathCorners = pathCorners
                }
            };
            return hasNavMesh;
        }
    }
}
