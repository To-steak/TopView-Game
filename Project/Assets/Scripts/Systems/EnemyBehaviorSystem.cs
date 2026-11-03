using System.Collections.Generic;
using TopView.Components;
using TopView.Entities;
using TopView.Requests;
using TopView.Utilities;
using UnityEngine;
using UnityEngine.AI;

namespace TopView.Systems
{
    // Idle과 Walk 상태의 적이 플레이어 감지로 무엇을 할지 정한다. 실제 이동은 EnemyNavigationSystem이, 공격 진행은 EnemyAttackStateSystem이 한다.
    public static class EnemyBehaviorSystem
    {
        /// <summary>
        /// Idle과 Walk 상태의 적마다 플레이어를 감지해 배회, 추적, 공격 시작을 정한다. EnemyStateChangeSystem 앞에 부른다.
        /// </summary>
        /// <param name="enemies">적 저장소</param>
        /// <param name="playerLocation">감지와 공격 대상인 플레이어의 위치와 반지름</param>
        /// <param name="random">World의 난수 생성기</param>
        /// <param name="stateChangeRequests">적 상태 변경 요청 큐</param>
        public static void Tick(EnemyStorage enemies, in PlayerLocationComponent playerLocation, System.Random random, Queue<EnemyStateChangeRequest> stateChangeRequests)
        {
            bool hasPlayer = playerLocation.hasPlayer;
            Vector3 playerPosition = playerLocation.position;
            float playerRadius = playerLocation.radius;
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
                bool isPlayerDetected = hasPlayer && TargetingUtility.IsWithin(position, playerPosition, behavior.detectionRadius + playerRadius);

                switch (actionState.state)
                {
                    case EnemyState.Idle:
                        if (!UpdateIdle(ref actionState, ref agent, ref rotation, position, in behavior, attacks, in identifier, isPlayerDetected, playerPosition, playerRadius, deltaTime, random, stateChangeRequests))
                        {
                            failedWanderCount++;
                        }
                        break;

                    case EnemyState.Walk:
                        // SetDestination이 도착 여부를 지우므로 먼저 읽어 둔다
                        bool hasReachedDestination = agent.hasReachedDestination;
                        if (isPlayerDetected)
                        {
                            if (TryBeginAttackInRange(ref actionState, position, attacks, in identifier, playerPosition, playerRadius, random, stateChangeRequests))
                            {
                                break;
                            }
                            EnemyNavigationUtility.SetDestination(ref agent, playerPosition, behavior.stopDistance);
                        }
                        if (hasReachedDestination)
                        {
                            if (isPlayerDetected && attacks.Length == 0)
                            {
                                missingAttackCount++;
                                missingAttackKind = entity.kind.kind;
                            }
                            stateChangeRequests.Enqueue(new EnemyStateChangeRequest { target = identifier, state = EnemyState.Idle });
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

        // 플레이어를 감지하면 고를 수 있는 공격을 하고, 없으면 멈추는 거리 밖일 때만 쫓는다. 멈추는 거리 안이면 플레이어 쪽으로 돌며 대기시간이 끝나기를 제자리에서 기다린다.
        // 감지하지 못했으면 배회 시간이 될 때마다 배회 지점으로 걷는다. 배회 지점을 고르지 못했으면 false를 돌려준다.
        private static bool UpdateIdle(ref EnemyActionStateComponent actionState, ref NavigationAgentComponent agent, ref RotationComponent rotation, Vector3 position, in EnemyBehaviorComponent behavior, EnemyAttackComponent[] attacks, in EntityIdentifier identifier, bool isPlayerDetected, Vector3 playerPosition, float playerRadius, float deltaTime, System.Random random, Queue<EnemyStateChangeRequest> stateChangeRequests)
        {
            if (isPlayerDetected)
            {
                if (TryBeginAttackInRange(ref actionState, position, attacks, in identifier, playerPosition, playerRadius, random, stateChangeRequests))
                {
                    return true;
                }

                if (TargetingUtility.IsWithin(position, playerPosition, behavior.stopDistance))
                {
                    TargetingUtility.RotateToward(ref rotation.current, position, playerPosition, behavior.angularSpeed * deltaTime);
                }
                else
                {
                    EnemyNavigationUtility.SetDestination(ref agent, playerPosition, behavior.stopDistance);
                    stateChangeRequests.Enqueue(new EnemyStateChangeRequest { target = identifier, state = EnemyState.Walk });
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

            EnemyNavigationUtility.SetDestination(ref agent, wanderPoint, 0f);
            stateChangeRequests.Enqueue(new EnemyStateChangeRequest { target = identifier, state = EnemyState.Walk });
            return true;
        }

        // 대기시간이 끝났고 사거리 안인 공격 중 무작위로 하나 골라 공격 상태를 요청한다. 그런 공격이 없으면 false를 돌려준다.
        private static bool TryBeginAttackInRange(ref EnemyActionStateComponent actionState, Vector3 position, EnemyAttackComponent[] attacks, in EntityIdentifier identifier, Vector3 playerPosition, float playerRadius, System.Random random, Queue<EnemyStateChangeRequest> stateChangeRequests)
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
            stateChangeRequests.Enqueue(new EnemyStateChangeRequest { target = identifier, state = EnemyState.Attack });
            return true;
        }

        private static bool IsAttackAvailable(in EnemyActionStateComponent actionState, EnemyAttackComponent[] attacks, int attackIndex, Vector3 position, Vector3 playerPosition, float playerRadius)
        {
            return actionState.attackCooldownRemainingTicks[attackIndex] <= 0
                && TargetingUtility.IsWithin(position, playerPosition, attacks[attackIndex].attackRange + playerRadius);
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
