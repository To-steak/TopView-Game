using System.Collections.Generic;
using TopView.Components;
using TopView.Entities;
using TopView.Records;
using UnityEngine;
using UnityEngine.AI;

namespace TopView.Systems
{
    // Idle과 Walk 상태의 적이 플레이어 감지로 무엇을 할지 정한다. 실제 이동은 EnemyNavigationSystem이, 공격 진행은 EnemyAttackStateSystem이 한다.
    public static class EnemyBehaviorSystem
    {
        /// <summary>
        /// Idle과 Walk 상태의 적마다 플레이어를 감지해 배회, 추적, 공격 시작을 정한다.
        /// </summary>
        /// <param name="enemies">적 저장소</param>
        /// <param name="players">플레이어 저장소. 감지와 공격 대상의 위치와 반지름을 읽는다</param>
        /// <param name="random">World의 난수 생성기</param>
        /// <param name="stateRecords">적 상태 변경 기록</param>
        public static void Tick(EnemyStorage enemies, PlayerStorage players, System.Random random, List<EnemyStateChangedRecord> stateRecords)
        {
            bool hasPlayer = TargetingSystem.TryGetPlayerTarget(players, out Vector3 playerPosition, out float playerRadius);
            float deltaTime = Time.fixedDeltaTime;
            int failedWanderCount = 0;
            int missingAttackCount = 0;
            int missingAttackKind = 0;

            for (int index = 0; index < enemies.entities.Length; index++)
            {
                ref EnemyEntity entity = ref enemies.entities[index];
                if (!entity.isAlive)
                {
                    continue;
                }

                ref EnemyActionStateComponent actionState = ref entity.actionState;
                ref NavigationAgentComponent agent = ref entity.agent;
                ref RotationComponent rotation = ref entity.rotation;
                ref readonly EnemyBehaviorComponent behavior = ref entity.behavior;
                EnemyAttackComponent[] attacks = entity.attackList.attacks;
                Vector3 position = entity.position.current;
                EntityIdentifier identifier = new EntityIdentifier { index = index, generation = entity.generation };
                bool isPlayerDetected = hasPlayer && TargetingSystem.IsWithin(position, playerPosition, behavior.detectionRadius + playerRadius);

                switch (actionState.state)
                {
                    case EnemyState.Idle:
                        if (!UpdateIdle(ref actionState, ref agent, ref rotation, position, in behavior, attacks, in identifier, isPlayerDetected, playerPosition, playerRadius, deltaTime, random, stateRecords))
                        {
                            failedWanderCount++;
                        }
                        break;

                    case EnemyState.Walk:
                        // SetDestination이 도착 여부를 지우므로 먼저 읽어 둔다
                        bool hasReachedDestination = agent.hasReachedDestination;
                        if (isPlayerDetected)
                        {
                            if (TryBeginAttackInRange(ref actionState, ref agent, position, attacks, in identifier, playerPosition, playerRadius, random, stateRecords))
                            {
                                break;
                            }
                            EnemyNavigationSystem.SetDestination(ref agent, playerPosition, behavior.stopDistance);
                        }
                        if (hasReachedDestination)
                        {
                            if (isPlayerDetected && attacks.Length == 0)
                            {
                                missingAttackCount++;
                                missingAttackKind = entity.kind.kind;
                            }
                            ChangeState(ref actionState, ref agent, attacks, in identifier, EnemyState.Idle, stateRecords);
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
        /// 지금 상태에서 나가고 새 상태에 들어가 상태 변경을 기록한다. Die 상태에서는 바꾸지 않는다. Attack 상태는 공격 번호를 정한 뒤 들어간다.
        /// </summary>
        /// <param name="actionState">적 행동 상태와 공격 진행</param>
        /// <param name="agent">목적지와 경로</param>
        /// <param name="attacks">공격 목록</param>
        /// <param name="identifier">적을 가리키는 값</param>
        /// <param name="newState">새 상태</param>
        /// <param name="stateRecords">적 상태 변경 기록</param>
        public static void ChangeState(ref EnemyActionStateComponent actionState, ref NavigationAgentComponent agent, EnemyAttackComponent[] attacks, in EntityIdentifier identifier, EnemyState newState, List<EnemyStateChangedRecord> stateRecords)
        {
            if (actionState.state == EnemyState.Die)
            {
                return;
            }

            stateRecords.Add(new EnemyStateChangedRecord
            {
                enemy = identifier,
                previousState = actionState.state,
                state = newState,
                attackKind = newState == EnemyState.Attack ? attacks[actionState.attackIndex].kind : default
            });

            actionState.state = newState;
            actionState.elapsedTicks = 0;

            switch (newState)
            {
                case EnemyState.Idle:
                    actionState.wanderElapsedTicks = 0;
                    EnemyNavigationSystem.StopMove(ref agent);
                    break;
                case EnemyState.Attack:
                    EnemyNavigationSystem.StopMove(ref agent);
                    break;
            }
        }

        // 플레이어를 감지하면 고를 수 있는 공격을 하고, 없으면 멈추는 거리 밖일 때만 쫓는다. 멈추는 거리 안이면 플레이어 쪽으로 돌며 대기시간이 끝나기를 제자리에서 기다린다.
        // 감지하지 못했으면 배회 시간이 될 때마다 배회 지점으로 걷는다. 배회 지점을 고르지 못했으면 false를 돌려준다.
        private static bool UpdateIdle(ref EnemyActionStateComponent actionState, ref NavigationAgentComponent agent, ref RotationComponent rotation, Vector3 position, in EnemyBehaviorComponent behavior, EnemyAttackComponent[] attacks, in EntityIdentifier identifier, bool isPlayerDetected, Vector3 playerPosition, float playerRadius, float deltaTime, System.Random random, List<EnemyStateChangedRecord> stateRecords)
        {
            if (isPlayerDetected)
            {
                if (TryBeginAttackInRange(ref actionState, ref agent, position, attacks, in identifier, playerPosition, playerRadius, random, stateRecords))
                {
                    return true;
                }

                if (TargetingSystem.IsWithin(position, playerPosition, behavior.stopDistance))
                {
                    TargetingSystem.RotateToward(ref rotation.current, position, playerPosition, behavior.angularSpeed * deltaTime);
                }
                else
                {
                    EnemyNavigationSystem.SetDestination(ref agent, playerPosition, behavior.stopDistance);
                    ChangeState(ref actionState, ref agent, attacks, in identifier, EnemyState.Walk, stateRecords);
                }
                return true;
            }

            actionState.wanderElapsedTicks++;
            if (actionState.wanderElapsedTicks < behavior.wanderIntervalTicks)
            {
                return true;
            }

            actionState.wanderElapsedTicks = 0;
            if (!TryGetWanderPoint(position, behavior.detectionRadius, random, out Vector3 wanderPoint))
            {
                return false;
            }

            EnemyNavigationSystem.SetDestination(ref agent, wanderPoint, 0f);
            ChangeState(ref actionState, ref agent, attacks, in identifier, EnemyState.Walk, stateRecords);
            return true;
        }

        // 대기시간이 끝났고 사거리 안인 공격 중 무작위로 하나 골라 공격 상태에 들어간다. 그런 공격이 없으면 false를 돌려준다.
        private static bool TryBeginAttackInRange(ref EnemyActionStateComponent actionState, ref NavigationAgentComponent agent, Vector3 position, EnemyAttackComponent[] attacks, in EntityIdentifier identifier, Vector3 playerPosition, float playerRadius, System.Random random, List<EnemyStateChangedRecord> stateRecords)
        {
            int inRangeCount = 0;
            for (int attackIndex = 0; attackIndex < attacks.Length; attackIndex++)
            {
                if (IsAttackAvailable(in actionState, attacks, attackIndex, position, playerPosition, playerRadius))
                {
                    inRangeCount++;
                }
            }

            if (inRangeCount == 0)
            {
                return false;
            }

            int remainingCount = random.Next(0, inRangeCount);
            for (int attackIndex = 0; attackIndex < attacks.Length; attackIndex++)
            {
                if (!IsAttackAvailable(in actionState, attacks, attackIndex, position, playerPosition, playerRadius))
                {
                    continue;
                }

                if (remainingCount == 0)
                {
                    actionState.attackIndex = attackIndex;
                    break;
                }
                remainingCount--;
            }

            actionState.isAttackCommitted = false;
            ChangeState(ref actionState, ref agent, attacks, in identifier, EnemyState.Attack, stateRecords);
            return true;
        }

        private static bool IsAttackAvailable(in EnemyActionStateComponent actionState, EnemyAttackComponent[] attacks, int attackIndex, Vector3 position, Vector3 playerPosition, float playerRadius)
        {
            return actionState.attackCooldownRemainingTicks[attackIndex] <= 0
                && TargetingSystem.IsWithin(position, playerPosition, attacks[attackIndex].attackRange + playerRadius);
        }

        // 지금 위치를 중심으로 감지 반지름 안의 무작위 점을 골라, 가장 가까운 NavMesh 위의 점을 돌려준다.
        private static bool TryGetWanderPoint(Vector3 position, float detectionRadius, System.Random random, out Vector3 wanderPoint)
        {
            Vector3 point = position + RandomInsideUnitSphere(random) * detectionRadius;
            if (NavMesh.SamplePosition(point, out NavMeshHit hit, detectionRadius, NavMesh.AllAreas))
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
