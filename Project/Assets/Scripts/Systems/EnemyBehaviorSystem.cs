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
            bool hasPlayer = TargetingSystem.TryGetPlayerTarget(players, out Vector3 playerPosition, out float playerRadius);
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
                if (enemy.health.current <= 0f && enemy.state != EnemyState.Die)
                {
                    ChangeState(ref enemy, in identifier, EnemyState.Die, stateRecords);
                }

                enemy.stateElapsedTicks++;
                DecreaseAttackCooldowns(ref enemy);
                bool isPlayerDetected = hasPlayer && TargetingSystem.IsWithin(enemy.position.current, playerPosition, enemy.behavior.detectionRadius + playerRadius);

                switch (enemy.state)
                {
                    case EnemyState.Idle:
                        if (!UpdateIdle(ref enemy, in identifier, isPlayerDetected, playerPosition, playerRadius, deltaTime, random, stateRecords))
                        {
                            failedWanderCount++;
                        }
                        break;

                    case EnemyState.Walk:
                        // SetDestination이 도착 여부를 지우므로 먼저 읽어 둔다
                        bool hasReachedDestination = enemy.hasReachedDestination;
                        if (isPlayerDetected)
                        {
                            if (TryBeginAttackInRange(ref enemy, in identifier, playerPosition, playerRadius, random, stateRecords))
                            {
                                break;
                            }
                            EnemyNavigationSystem.SetDestination(ref enemy, playerPosition, enemy.behavior.stopDistance);
                        }
                        if (hasReachedDestination)
                        {
                            if (isPlayerDetected && enemy.attacks.Length == 0)
                            {
                                missingAttackCount++;
                                missingAttackKind = enemy.kind;
                            }
                            ChangeState(ref enemy, in identifier, EnemyState.Idle, stateRecords);
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

        // 지금 상태에서 나가고 새 상태에 들어간다. Die 상태에서는 바꾸지 않는다. Attack 상태는 TryBeginAttackInRange로 들어간다.
        private static void ChangeState(ref EnemyEntity enemy, in EntityIdentifier identifier, EnemyState newState, List<EnemyStateChangedRecord> stateRecords)
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
                    EnemyNavigationSystem.StopMove(ref enemy);
                    break;
                case EnemyState.Attack:
                case EnemyState.Die:
                    EnemyNavigationSystem.StopMove(ref enemy);
                    break;
            }
        }

        // 플레이어를 감지하면 고를 수 있는 공격을 하고, 없으면 멈추는 거리 밖일 때만 쫓는다. 멈추는 거리 안이면 플레이어 쪽으로 돌며 대기시간이 끝나기를 제자리에서 기다린다.
        // 감지하지 못했으면 배회 시간이 될 때마다 배회 지점으로 걷는다. 배회 지점을 고르지 못했으면 false를 돌려준다.
        private static bool UpdateIdle(ref EnemyEntity enemy, in EntityIdentifier identifier, bool isPlayerDetected, Vector3 playerPosition, float playerRadius, float deltaTime, System.Random random, List<EnemyStateChangedRecord> stateRecords)
        {
            if (isPlayerDetected)
            {
                if (TryBeginAttackInRange(ref enemy, in identifier, playerPosition, playerRadius, random, stateRecords))
                {
                    return true;
                }

                if (TargetingSystem.IsWithin(enemy.position.current, playerPosition, enemy.behavior.stopDistance))
                {
                    TargetingSystem.RotateToward(ref enemy.rotation.current, enemy.position.current, playerPosition, enemy.behavior.angularSpeed * deltaTime);
                }
                else
                {
                    EnemyNavigationSystem.SetDestination(ref enemy, playerPosition, enemy.behavior.stopDistance);
                    ChangeState(ref enemy, in identifier, EnemyState.Walk, stateRecords);
                }
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

            EnemyNavigationSystem.SetDestination(ref enemy, wanderPoint, 0f);
            ChangeState(ref enemy, in identifier, EnemyState.Walk, stateRecords);
            return true;
        }

        private static void UpdateAttack(ref EnemyEntity enemy, in EntityIdentifier identifier, bool hasPlayer, Vector3 playerPosition, float deltaTime, List<EnemyStateChangedRecord> stateRecords, Queue<ProjectileSpawnRequest> projectileSpawnRequests, Queue<MeleeSpawnRequest> meleeSpawnRequests)
        {
            // 공격이 나가기 전까지 플레이어 쪽으로 돈다
            if (!enemy.isAttackCommitted && hasPlayer)
            {
                TargetingSystem.RotateToward(ref enemy.rotation.current, enemy.position.current, playerPosition, enemy.behavior.angularSpeed * deltaTime);
            }

            ref readonly EnemyAttackComponent attack = ref enemy.attacks[enemy.attackIndex];
            if (!enemy.isAttackCommitted && enemy.stateElapsedTicks >= attack.startTicks)
            {
                enemy.isAttackCommitted = true;
                int missileShotCount = 0;
                EntityReference owner = new EntityReference { kind = EntityKind.Enemy, identifier = identifier };
                EnemyAttackSystem.CommitAttack(in attack, enemy.position.current, enemy.rotation.current, enemy.attackDamage, ref missileShotCount, in owner, projectileSpawnRequests, meleeSpawnRequests);
            }

            if (EnemyAttackSystem.IsAttackFinished(in attack, enemy.stateElapsedTicks, enemy.isAttackCommitted))
            {
                enemy.attackCooldownRemainingTicks[enemy.attackIndex] = attack.cooldownTicks;
                ChangeState(ref enemy, in identifier, EnemyState.Idle, stateRecords);
            }
        }

        private static void DecreaseAttackCooldowns(ref EnemyEntity enemy)
        {
            for (int attackIndex = 0; attackIndex < enemy.attackCooldownRemainingTicks.Length; attackIndex++)
            {
                if (enemy.attackCooldownRemainingTicks[attackIndex] > 0)
                {
                    enemy.attackCooldownRemainingTicks[attackIndex]--;
                }
            }
        }

        // 대기시간이 끝났고 사거리 안인 공격 중 무작위로 하나 골라 공격 상태에 들어간다. 그런 공격이 없으면 false를 돌려준다.
        private static bool TryBeginAttackInRange(ref EnemyEntity enemy, in EntityIdentifier identifier, Vector3 playerPosition, float playerRadius, System.Random random, List<EnemyStateChangedRecord> stateRecords)
        {
            int inRangeCount = 0;
            for (int attackIndex = 0; attackIndex < enemy.attacks.Length; attackIndex++)
            {
                if (IsAttackAvailable(in enemy, attackIndex, playerPosition, playerRadius))
                {
                    inRangeCount++;
                }
            }

            if (inRangeCount == 0)
            {
                return false;
            }

            int remainingCount = random.Next(0, inRangeCount);
            for (int attackIndex = 0; attackIndex < enemy.attacks.Length; attackIndex++)
            {
                if (!IsAttackAvailable(in enemy, attackIndex, playerPosition, playerRadius))
                {
                    continue;
                }

                if (remainingCount == 0)
                {
                    enemy.attackIndex = attackIndex;
                    break;
                }
                remainingCount--;
            }

            enemy.isAttackCommitted = false;
            ChangeState(ref enemy, in identifier, EnemyState.Attack, stateRecords);
            return true;
        }

        private static bool IsAttackAvailable(in EnemyEntity enemy, int attackIndex, Vector3 playerPosition, float playerRadius)
        {
            return enemy.attackCooldownRemainingTicks[attackIndex] <= 0
                && TargetingSystem.IsWithin(enemy.position.current, playerPosition, enemy.attacks[attackIndex].attackRange + playerRadius);
        }

        // 지금 위치를 중심으로 감지 반지름 안의 무작위 점을 골라, 가장 가까운 NavMesh 위의 점을 돌려준다.
        private static bool TryGetWanderPoint(in EnemyEntity enemy, System.Random random, out Vector3 wanderPoint)
        {
            Vector3 point = enemy.position.current + RandomInsideUnitSphere(random) * enemy.behavior.detectionRadius;
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
    }
}
