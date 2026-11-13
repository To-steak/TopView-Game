using System.Collections.Generic;
using TopView.Components;
using TopView.Entities;
using TopView.Requests;
using TopView.Utilities;
using UnityEngine;

namespace TopView.Systems
{
    // Idle과 Walk 상태의 적이 플레이어를 어떻게 쫓고 공격할지 정한다. 실제 이동은 EnemyNavigationSystem이, 공격 진행은 EnemyAttackStateSystem이 한다.
    public static class EnemyBehaviorSystem
    {
        // 처리할 적이 가진 컴포넌트
        private const ComponentMask ENEMY_MASK = ComponentMask.Position | ComponentMask.Rotation | ComponentMask.EnemyKind | ComponentMask.NavigationMovement
            | ComponentMask.AttackList | ComponentMask.EnemyActionState | ComponentMask.NavigationAgent;

        /// <summary>
        /// Idle과 Walk 상태의 적마다 플레이어 추적과 공격 시작을 정한다. 플레이어가 없으면 새 행동을 정하지 않는다. EnemyStateChangeSystem 앞에 부른다.
        /// </summary>
        /// <param name="storage">엔티티 저장소. 적(Position, Rotation, EnemyKind, NavigationMovement, AttackList, EnemyActionState, NavigationAgent)만 처리하고, 회전, 공격 번호, 목적지를 고친다</param>
        /// <param name="playerLocation">추적과 공격 대상인 플레이어의 위치와 반지름</param>
        /// <param name="random">World의 난수 생성기</param>
        /// <param name="stateChangeRequests">적 상태 변경 요청 큐</param>
        public static void Tick(EntityStorage storage, in PlayerLocationComponent playerLocation, System.Random random, Queue<EnemyStateChangeRequest> stateChangeRequests)
        {
            bool hasPlayer = playerLocation.hasPlayer;
            Vector3 playerPosition = playerLocation.position;
            float playerRadius = playerLocation.radius;
            float deltaTime = Time.fixedDeltaTime;
            int missingAttackCount = 0;
            int missingAttackKind = 0;

            for (int index = 0; index < storage.entities.Length; index++)
            {
                ref Entity entity = ref storage.entities[index];
                if ((entity.mask & ENEMY_MASK) != ENEMY_MASK)
                {
                    continue;
                }

                ref EnemyActionStateComponent actionState = ref entity.enemyActionState;
                ref NavigationAgentComponent agent = ref entity.navigationAgent;
                ref RotationComponent rotation = ref entity.rotation;
                ref readonly NavigationMovementComponent movement = ref entity.navigationMovement;
                EnemyAttackComponent[] attacks = entity.attackList.attacks;
                Vector3 position = entity.position.current;
                EntityIdentifier identifier = new EntityIdentifier { index = index, generation = entity.generation };

                switch (actionState.state)
                {
                    case EnemyState.Idle:
                        if (hasPlayer)
                        {
                            UpdateIdle(ref actionState, ref agent, ref rotation, position, in movement, attacks, in identifier, playerPosition, playerRadius, deltaTime, random, stateChangeRequests);
                        }
                        break;

                    case EnemyState.Walk:
                        // SetDestination이 도착 여부를 지우므로 먼저 읽어 둔다
                        bool hasReachedDestination = agent.hasReachedDestination;
                        if (hasPlayer)
                        {
                            if (TryBeginAttackInRange(ref actionState, position, attacks, in identifier, playerPosition, playerRadius, random, stateChangeRequests))
                            {
                                break;
                            }
                            EnemyNavigationUtility.SetDestination(ref agent, playerPosition, movement.stopDistance);
                        }
                        if (hasReachedDestination)
                        {
                            if (hasPlayer && attacks.Length == 0)
                            {
                                missingAttackCount++;
                                missingAttackKind = entity.enemyKind.kind;
                            }
                            stateChangeRequests.Enqueue(new EnemyStateChangeRequest { target = identifier, state = EnemyState.Idle });
                        }
                        break;
                }
            }

            if (missingAttackCount > 0)
            {
                Debug.LogWarning($"EnemyBehaviorSystem >> 공격이 없어 공격하지 않은 적 {missingAttackCount}마리가 있다. 마지막 적 종류: {missingAttackKind}");
            }
        }

        // 고를 수 있는 공격을 하고, 없으면 멈추는 거리 밖일 때만 쫓는다. 멈추는 거리 안이면 플레이어 쪽으로 돌며 대기시간이 끝나기를 제자리에서 기다린다.
        private static void UpdateIdle(ref EnemyActionStateComponent actionState, ref NavigationAgentComponent agent, ref RotationComponent rotation, Vector3 position, in NavigationMovementComponent movement, EnemyAttackComponent[] attacks, in EntityIdentifier identifier, Vector3 playerPosition, float playerRadius, float deltaTime, System.Random random, Queue<EnemyStateChangeRequest> stateChangeRequests)
        {
            if (TryBeginAttackInRange(ref actionState, position, attacks, in identifier, playerPosition, playerRadius, random, stateChangeRequests))
            {
                return;
            }

            if (TargetingUtility.IsWithin(position, playerPosition, movement.stopDistance))
            {
                TargetingUtility.RotateToward(ref rotation.current, position, playerPosition, movement.angularSpeed * deltaTime);
            }
            else
            {
                EnemyNavigationUtility.SetDestination(ref agent, playerPosition, movement.stopDistance);
                stateChangeRequests.Enqueue(new EnemyStateChangeRequest { target = identifier, state = EnemyState.Walk });
            }
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

            actionState.committedShotCount = 0;
            stateChangeRequests.Enqueue(new EnemyStateChangeRequest { target = identifier, state = EnemyState.Attack });
            return true;
        }

        private static bool IsAttackAvailable(in EnemyActionStateComponent actionState, EnemyAttackComponent[] attacks, int attackIndex, Vector3 position, Vector3 playerPosition, float playerRadius)
        {
            return actionState.attackCooldownRemainingTicks[attackIndex] <= 0
                && TargetingUtility.IsWithin(position, playerPosition, attacks[attackIndex].attackRange + playerRadius);
        }
    }
}
