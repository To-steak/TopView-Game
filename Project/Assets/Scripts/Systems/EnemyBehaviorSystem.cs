using System.Collections.Generic;
using TopView.Components;
using TopView.Entities;
using TopView.Records;
using TopView.Requests;
using UnityEngine;
using UnityEngine.AI;

namespace TopView.Systems
{
    // 적의 행동 상태를 플레이어 감지와 상태 타이머로 바꾸고, 목적지와 공격을 정한다. 실제 이동은 EnemyNavigationSystem이 한다.
    public static class EnemyBehaviorSystem
    {
        /// <summary>
        /// 살아 있는 적마다 체력이 0이면 사망 상태로 바꾸고, 상태 시간을 1틱 늘린 뒤 상태별로 감지·배회·추적·공격·소멸을 처리한다.
        /// </summary>
        /// <param name="enemies">적 저장소</param>
        /// <param name="players">플레이어 저장소. 감지와 공격 대상이다</param>
        /// <param name="random">World의 난수 생성기</param>
        /// <param name="stateRecords">적 상태 변경 기록</param>
        /// <param name="projectileSpawnRequests">투사체 생성 요청 큐</param>
        /// <param name="meleeSpawnRequests">근접 판정 생성 요청 큐</param>
        public static void Tick(EnemyStorage enemies, PlayerStorage players, System.Random random, List<EnemyStateChangedRecord> stateRecords, Queue<ProjectileSpawnRequest> projectileSpawnRequests, Queue<MeleeSpawnRequest> meleeSpawnRequests)
        {
            bool hasPlayer = TargetingSystem.TryGetPlayerTarget(players, out EntityIdentifier _, out Vector3 playerPosition, out float playerRadius);
            float deltaTime = Time.fixedDeltaTime;
            int failedWanderCount = 0;
            int missingAttackCount = 0;
            int missingAttackKind = 0;

            for (int enemyIndex = 0; enemyIndex < enemies.entities.Length; enemyIndex++)
            {
                ref EnemyEntity enemy = ref enemies.entities[enemyIndex];
                if (!enemy.isAlive)
                {
                    continue;
                }

                EntityIdentifier identifier = new EntityIdentifier { index = enemyIndex, generation = enemy.generation };
                if (enemy.health <= 0f && enemy.state != EnemyState.Die)
                {
                    ChangeState(ref enemy, in identifier, EnemyState.Die, stateRecords);
                }

                enemy.stateElapsedTicks++;
                bool isPlayerDetected = hasPlayer && TargetingSystem.IsWithin(enemy.currentPosition, playerPosition, enemy.behavior.detectionRadius + playerRadius);

                switch (enemy.state)
                {
                    case EnemyState.Idle:
                        if (!UpdateIdle(ref enemy, in identifier, isPlayerDetected, playerPosition, random, stateRecords))
                        {
                            failedWanderCount++;
                        }
                        break;

                    case EnemyState.Walk:
                        bool hasReachedDestination = enemy.hasReachedDestination;
                        if (isPlayerDetected)
                        {
                            SetDestination(ref enemy, playerPosition, enemy.behavior.attackDistance);
                        }
                        if (hasReachedDestination)
                        {
                            if (isPlayerDetected)
                            {
                                if (!BeginAttack(ref enemy, in identifier, random, stateRecords))
                                {
                                    missingAttackCount++;
                                    missingAttackKind = enemy.kind;
                                }
                            }
                            else
                            {
                                ChangeState(ref enemy, in identifier, EnemyState.Idle, stateRecords);
                            }
                        }
                        break;

                    case EnemyState.Attack:
                        UpdateAttack(ref enemy, in identifier, hasPlayer, playerPosition, deltaTime, stateRecords, projectileSpawnRequests, meleeSpawnRequests);
                        break;

                    case EnemyState.Die:
                        if (enemy.stateElapsedTicks >= enemy.behavior.dieTicks)
                        {
                            enemy.isAlive = false;
                            enemies.freeIndicesByKind[enemy.kind].Add(enemyIndex);
                        }
                        break;
                }
            }

            if (failedWanderCount > 0)
            {
                Debug.LogWarning($"EnemyBehaviorSystem >> 근처에 NavMesh가 없어 배회 지점을 고르지 못한 적 {failedWanderCount}마리가 있다.");
            }
            if (missingAttackCount > 0)
            {
                Debug.LogWarning($"EnemyBehaviorSystem >> 공격이 없어 공격하지 않은 적 {missingAttackCount}마리가 있다. 마지막 적 종류: {missingAttackKind}");
            }
        }

        /// <summary>
        /// 지금 상태에서 나가고 새 상태에 들어간다. Die 상태에서는 바꾸지 않는다. Attack 상태는 BeginAttack으로 들어간다.
        /// </summary>
        /// <param name="enemy">적</param>
        /// <param name="identifier">적을 가리키는 값</param>
        /// <param name="newState">새 상태</param>
        /// <param name="stateRecords">적 상태 변경 기록</param>
        public static void ChangeState(ref EnemyEntity enemy, in EntityIdentifier identifier, EnemyState newState, List<EnemyStateChangedRecord> stateRecords)
        {
            if (enemy.state == EnemyState.Die)
            {
                return;
            }

            stateRecords.Add(new EnemyStateChangedRecord
            {
                enemy = identifier,
                previousState = enemy.state,
                state = newState,
                attackKind = newState == EnemyState.Attack ? enemy.attacks[enemy.attackIndex].kind : default
            });

            enemy.state = newState;
            enemy.stateElapsedTicks = 0;

            switch (newState)
            {
                case EnemyState.Idle:
                    enemy.wanderElapsedTicks = 0;
                    break;
                case EnemyState.Attack:
                case EnemyState.Die:
                    StopMove(ref enemy);
                    break;
            }
        }

        // 플레이어를 감지하면 쫓고, 아니면 배회 시간이 될 때마다 배회 지점으로 걷는다. 배회 지점을 고르지 못했으면 false를 돌려준다.
        private static bool UpdateIdle(ref EnemyEntity enemy, in EntityIdentifier identifier, bool isPlayerDetected, Vector3 playerPosition, System.Random random, List<EnemyStateChangedRecord> stateRecords)
        {
            if (isPlayerDetected)
            {
                SetDestination(ref enemy, playerPosition, enemy.behavior.attackDistance);
                ChangeState(ref enemy, in identifier, EnemyState.Walk, stateRecords);
                return true;
            }

            enemy.wanderElapsedTicks++;
            if (enemy.wanderElapsedTicks < enemy.behavior.wanderIntervalTicks)
            {
                return true;
            }

            enemy.wanderElapsedTicks = 0;
            if (!TryGetWanderPoint(in enemy, random, out Vector3 wanderPoint))
            {
                return false;
            }

            SetDestination(ref enemy, wanderPoint, 0f);
            ChangeState(ref enemy, in identifier, EnemyState.Walk, stateRecords);
            return true;
        }

        private static void UpdateAttack(ref EnemyEntity enemy, in EntityIdentifier identifier, bool hasPlayer, Vector3 playerPosition, float deltaTime, List<EnemyStateChangedRecord> stateRecords, Queue<ProjectileSpawnRequest> projectileSpawnRequests, Queue<MeleeSpawnRequest> meleeSpawnRequests)
        {
            if (enemy.hasAttackTarget && !enemy.isAttackCommitted && hasPlayer)
            {
                TargetingSystem.TurnToward(ref enemy.currentRotation, enemy.currentPosition, playerPosition, deltaTime * enemy.behavior.attackRotationSpeed);
            }

            ref readonly EnemyAttackComponent attack = ref enemy.attacks[enemy.attackIndex];
            if (!enemy.isAttackCommitted && enemy.stateElapsedTicks >= attack.commitTicks)
            {
                enemy.isAttackCommitted = true;
                int missileShotCount = 0;
                EntityReference owner = new EntityReference { kind = EntityKind.Enemy, identifier = identifier };
                EnemyAttackSystem.CommitAttack(in attack, enemy.currentPosition, enemy.currentRotation, enemy.attackDamage, ref missileShotCount, in owner, projectileSpawnRequests, meleeSpawnRequests);
            }

            if (enemy.stateElapsedTicks >= attack.durationTicks)
            {
                ChangeState(ref enemy, in identifier, EnemyState.Idle, stateRecords);
            }
        }

        // 공격을 무작위로 하나 골라 공격 상태에 들어간다. 공격이 없으면 Idle로 돌아가고 false를 돌려준다.
        private static bool BeginAttack(ref EnemyEntity enemy, in EntityIdentifier identifier, System.Random random, List<EnemyStateChangedRecord> stateRecords)
        {
            if (enemy.attacks.Length == 0)
            {
                ChangeState(ref enemy, in identifier, EnemyState.Idle, stateRecords);
                return false;
            }

            enemy.attackIndex = random.Next(0, enemy.attacks.Length);
            enemy.isAttackCommitted = false;
            enemy.hasAttackTarget = true;
            ChangeState(ref enemy, in identifier, EnemyState.Attack, stateRecords);
            return true;
        }

        // 지금 위치를 중심으로 감지 반지름 안의 무작위 점을 골라, 가장 가까운 NavMesh 위의 점을 돌려준다.
        private static bool TryGetWanderPoint(in EnemyEntity enemy, System.Random random, out Vector3 wanderPoint)
        {
            Vector3 point = enemy.currentPosition + RandomInsideUnitSphere(random) * enemy.behavior.detectionRadius;
            if (NavMesh.SamplePosition(point, out NavMeshHit hit, enemy.behavior.detectionRadius, NavMesh.AllAreas))
            {
                wanderPoint = hit.position;
                return true;
            }

            wanderPoint = Vector3.zero;
            return false;
        }

        private static Vector3 RandomInsideUnitSphere(System.Random random)
        {
            Vector3 point;
            do
            {
                point = new Vector3(
                    (float)random.NextDouble() * 2f - 1f,
                    (float)random.NextDouble() * 2f - 1f,
                    (float)random.NextDouble() * 2f - 1f);
            }
            while (point.sqrMagnitude > 1f);

            return point;
        }

        private static void SetDestination(ref EnemyEntity enemy, Vector3 destination, float stoppingDistance)
        {
            enemy.hasDestination = true;
            enemy.destination = destination;
            enemy.stoppingDistance = stoppingDistance;
            enemy.isPathDirty = true;
            enemy.hasReachedDestination = false;
        }

        private static void StopMove(ref EnemyEntity enemy)
        {
            enemy.hasDestination = false;
            enemy.isPathDirty = false;
            enemy.hasReachedDestination = false;
            enemy.pathCornerCount = 0;
            enemy.pathCornerIndex = 0;
            enemy.velocity = Vector3.zero;
        }
    }
}
