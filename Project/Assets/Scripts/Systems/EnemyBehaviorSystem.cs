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
        /// 적마다 체력이 0이면 사망 상태로 바꾸고, 상태 시간을 1틱 늘린 뒤 상태별로 감지·배회·추적·공격·소멸을 처리한다.
        /// </summary>
        /// <param name="masks">칸별 컴포넌트 표시</param>
        /// <param name="generations">칸이 채워진 횟수</param>
        /// <param name="kinds">적 종류 번호</param>
        /// <param name="healths">체력</param>
        /// <param name="combats">레벨과 공격력</param>
        /// <param name="positions">위치</param>
        /// <param name="rotations">회전</param>
        /// <param name="attackLists">공격 목록</param>
        /// <param name="behaviors">적 행동 값</param>
        /// <param name="actionStates">적 행동 상태와 공격 진행</param>
        /// <param name="agents">목적지와 경로</param>
        /// <param name="freeIndicesByKind">적 종류별 구간의 빈 칸 번호 목록</param>
        /// <param name="players">플레이어 저장소. 감지와 공격 대상이다</param>
        /// <param name="random">World의 난수 생성기</param>
        /// <param name="stateRecords">적 상태 변경 기록</param>
        /// <param name="projectileSpawnRequests">투사체 생성 요청 큐</param>
        /// <param name="meleeSpawnRequests">근접 판정 생성 요청 큐</param>
        public static void Tick(ComponentMask[] masks, int[] generations, EnemyKindComponent[] kinds, HealthComponent[] healths, CombatComponent[] combats, PositionComponent[] positions, RotationComponent[] rotations, AttackListComponent[] attackLists, EnemyBehaviorComponent[] behaviors, EnemyActionStateComponent[] actionStates, NavigationAgentComponent[] agents, List<int>[] freeIndicesByKind, PlayerStorage players, System.Random random, List<EnemyStateChangedRecord> stateRecords, Queue<ProjectileSpawnRequest> projectileSpawnRequests, Queue<MeleeSpawnRequest> meleeSpawnRequests)
        {
            bool hasPlayer = TargetingSystem.TryGetPlayerTarget(players, out Vector3 playerPosition, out float playerRadius);
            float deltaTime = Time.fixedDeltaTime;
            int failedWanderCount = 0;
            int missingAttackCount = 0;
            int missingAttackKind = 0;

            for (int index = 0; index < masks.Length; index++)
            {
                if ((masks[index] & ComponentMask.EnemyArchetype) != ComponentMask.EnemyArchetype)
                {
                    continue;
                }

                ref EnemyActionStateComponent actionState = ref actionStates[index];
                ref NavigationAgentComponent agent = ref agents[index];
                ref RotationComponent rotation = ref rotations[index];
                ref readonly EnemyBehaviorComponent behavior = ref behaviors[index];
                EnemyAttackComponent[] attacks = attackLists[index].attacks;
                Vector3 position = positions[index].current;

                EntityIdentifier identifier = new EntityIdentifier { index = index, generation = generations[index] };
                if (healths[index].current <= 0f && actionState.state != EnemyState.Die)
                {
                    ChangeState(ref actionState, ref agent, attacks, in identifier, EnemyState.Die, stateRecords);
                }

                actionState.elapsedTicks++;
                DecreaseAttackCooldowns(ref actionState);
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
                                missingAttackKind = kinds[index].kind;
                            }
                            ChangeState(ref actionState, ref agent, attacks, in identifier, EnemyState.Idle, stateRecords);
                        }
                        break;

                    case EnemyState.Attack:
                        UpdateAttack(ref actionState, ref agent, ref rotation, position, in behavior, combats[index].attackDamage, attacks, in identifier, hasPlayer, playerPosition, deltaTime, stateRecords, projectileSpawnRequests, meleeSpawnRequests);
                        break;

                    case EnemyState.Die:
                        if (actionState.elapsedTicks >= behavior.dieTicks)
                        {
                            SpawnSystem.ReleaseSlot(masks, freeIndicesByKind[kinds[index].kind], index);
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
        private static void ChangeState(ref EnemyActionStateComponent actionState, ref NavigationAgentComponent agent, EnemyAttackComponent[] attacks, in EntityIdentifier identifier, EnemyState newState, List<EnemyStateChangedRecord> stateRecords)
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
                case EnemyState.Die:
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

        private static void UpdateAttack(ref EnemyActionStateComponent actionState, ref NavigationAgentComponent agent, ref RotationComponent rotation, Vector3 position, in EnemyBehaviorComponent behavior, float attackDamage, EnemyAttackComponent[] attacks, in EntityIdentifier identifier, bool hasPlayer, Vector3 playerPosition, float deltaTime, List<EnemyStateChangedRecord> stateRecords, Queue<ProjectileSpawnRequest> projectileSpawnRequests, Queue<MeleeSpawnRequest> meleeSpawnRequests)
        {
            // 공격이 나가기 전까지 플레이어 쪽으로 돈다
            if (!actionState.isAttackCommitted && hasPlayer)
            {
                TargetingSystem.RotateToward(ref rotation.current, position, playerPosition, behavior.angularSpeed * deltaTime);
            }

            ref readonly EnemyAttackComponent attack = ref attacks[actionState.attackIndex];
            if (!actionState.isAttackCommitted && actionState.elapsedTicks >= attack.startTicks)
            {
                actionState.isAttackCommitted = true;
                int missileShotCount = 0;
                EntityReference owner = new EntityReference { kind = EntityKind.Shared, identifier = identifier };
                EnemyAttackSystem.CommitAttack(in attack, position, rotation.current, attackDamage, ref missileShotCount, in owner, projectileSpawnRequests, meleeSpawnRequests);
            }

            if (EnemyAttackSystem.IsAttackFinished(in attack, actionState.elapsedTicks, actionState.isAttackCommitted))
            {
                actionState.attackCooldownRemainingTicks[actionState.attackIndex] = attack.cooldownTicks;
                ChangeState(ref actionState, ref agent, attacks, in identifier, EnemyState.Idle, stateRecords);
            }
        }

        private static void DecreaseAttackCooldowns(ref EnemyActionStateComponent actionState)
        {
            for (int attackIndex = 0; attackIndex < actionState.attackCooldownRemainingTicks.Length; attackIndex++)
            {
                if (actionState.attackCooldownRemainingTicks[attackIndex] > 0)
                {
                    actionState.attackCooldownRemainingTicks[attackIndex]--;
                }
            }
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
