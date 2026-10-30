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
        // 이보다 가까운 방향으로는 돌지 않는다(유닛²)
        private const float MIN_TURN_DISTANCE_SQUARED = 0.001f;

        /// <summary>
        /// 살아 있는 적마다 상태 시간을 1틱 늘리고, 상태별로 감지·배회·추적·공격·소멸을 처리한다.
        /// </summary>
        /// <param name="enemies">적 저장소</param>
        /// <param name="players">플레이어 저장소. 감지와 공격 대상이다</param>
        /// <param name="random">World의 난수 생성기</param>
        /// <param name="stateRecords">적 상태 변경 기록</param>
        /// <param name="projectileSpawnRequests">투사체 생성 요청 큐</param>
        /// <param name="playerDamageRequests">플레이어 피해 요청 큐</param>
        public static void Tick(EnemyStorage enemies, PlayerStorage players, System.Random random, List<EnemyStateChangedRecord> stateRecords, Queue<ProjectileSpawnRequest> projectileSpawnRequests, Queue<PlayerDamageRequest> playerDamageRequests)
        {
            bool hasPlayer = TryGetPlayerTarget(players, out EntityIdentifier playerIdentifier, out Vector3 playerPosition, out float playerRadius);
            float deltaTime = Time.fixedDeltaTime;
            int failedWanderCount = 0;

            for (int enemyIndex = 0; enemyIndex < enemies.entities.Length; enemyIndex++)
            {
                ref EnemyEntity enemy = ref enemies.entities[enemyIndex];
                if (!enemy.isAlive)
                {
                    continue;
                }

                EntityIdentifier identifier = new EntityIdentifier { index = enemyIndex, generation = enemy.generation };
                enemy.stateElapsedTicks++;
                bool isPlayerDetected = hasPlayer && IsWithin(enemy.currentPosition, playerPosition, enemy.behavior.detectionRadius + playerRadius);

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
                                BeginAttack(ref enemy, in identifier, random, stateRecords);
                            }
                            else
                            {
                                ChangeState(ref enemy, in identifier, EnemyState.Idle, stateRecords);
                            }
                        }
                        break;

                    case EnemyState.Attack:
                        UpdateAttack(ref enemy, in identifier, hasPlayer, in playerIdentifier, playerPosition, playerRadius, deltaTime, stateRecords, projectileSpawnRequests, playerDamageRequests);
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

        /// <summary>
        /// 살아 있는 플레이어의 위치와 피격 반지름을 돌려준다.
        /// </summary>
        /// <param name="players">플레이어 저장소</param>
        /// <param name="playerIdentifier">플레이어를 가리키는 값</param>
        /// <param name="playerPosition">플레이어 위치(월드)</param>
        /// <param name="playerRadius">플레이어 이동 충돌 캡슐의 반지름. 감지와 공격 범위에 더한다</param>
        /// <returns>살아 있는 플레이어가 있으면 true</returns>
        public static bool TryGetPlayerTarget(PlayerStorage players, out EntityIdentifier playerIdentifier, out Vector3 playerPosition, out float playerRadius)
        {
            for (int playerIndex = 0; playerIndex < players.entities.Length; playerIndex++)
            {
                ref readonly PlayerEntity player = ref players.entities[playerIndex];
                if (!player.isAlive)
                {
                    continue;
                }

                playerIdentifier = new EntityIdentifier { index = playerIndex, generation = player.generation };
                playerPosition = player.currentPosition;
                playerRadius = player.movement.capsuleRadius;
                return true;
            }

            playerIdentifier = default;
            playerPosition = Vector3.zero;
            playerRadius = 0f;
            return false;
        }

        /// <summary>
        /// 공격 하나를 내보낸다. Body는 범위 안의 플레이어에게 피해 요청을 넣고, 나머지는 투사체 생성 요청을 넣는다.
        /// </summary>
        /// <param name="attack">공격 값</param>
        /// <param name="position">공격하는 쪽의 위치(월드)</param>
        /// <param name="rotation">공격하는 쪽의 회전(월드)</param>
        /// <param name="attackDamage">공격하는 쪽의 공격력</param>
        /// <param name="missileShotCount">BossMissile을 쏜 횟수. 짝수·홀수로 발사 위치를 고르고 1 늘린다</param>
        /// <param name="hasPlayer">살아 있는 플레이어가 있는지</param>
        /// <param name="playerIdentifier">플레이어를 가리키는 값</param>
        /// <param name="playerPosition">플레이어 위치(월드)</param>
        /// <param name="playerRadius">플레이어 피격 반지름</param>
        /// <param name="projectileSpawnRequests">투사체 생성 요청 큐</param>
        /// <param name="playerDamageRequests">플레이어 피해 요청 큐</param>
        public static void CommitAttack(in EnemyAttackComponent attack, Vector3 position, Quaternion rotation, float attackDamage, ref int missileShotCount, bool hasPlayer, in EntityIdentifier playerIdentifier, Vector3 playerPosition, float playerRadius, Queue<ProjectileSpawnRequest> projectileSpawnRequests, Queue<PlayerDamageRequest> playerDamageRequests)
        {
            float damage = attackDamage * attack.damageMultiplier;

            if (attack.kind == EnemyAttackKind.Body)
            {
                Vector3 center = position + rotation * Vector3.forward * attack.bodyOffset;
                if (hasPlayer && IsWithin(center, playerPosition, attack.bodyRadius + playerRadius))
                {
                    playerDamageRequests.Enqueue(new PlayerDamageRequest { target = playerIdentifier, amount = damage });
                }
                return;
            }

            Vector3 spawnOffset = attack.spawnOffset;
            if (attack.kind == EnemyAttackKind.BossMissile)
            {
                spawnOffset = missileShotCount % 2 == 0 ? attack.spawnOffset : attack.alternateSpawnOffset;
                missileShotCount++;
            }

            projectileSpawnRequests.Enqueue(new ProjectileSpawnRequest
            {
                projectile = attack.projectile,
                isTargetingPlayer = true,
                damage = damage,
                position = position + rotation * spawnOffset,
                rotation = rotation,
                velocity = rotation * Vector3.forward * attack.projectile.speed
            });
        }

        /// <summary>
        /// 목표 쪽(XZ)으로 남은 각도의 일부만큼 돈다.
        /// </summary>
        /// <param name="rotation">돌릴 회전</param>
        /// <param name="position">도는 쪽의 위치(월드)</param>
        /// <param name="targetPosition">바라볼 위치(월드)</param>
        /// <param name="fraction">이번 틱에 돌 비율(0~1)</param>
        public static void TurnToward(ref Quaternion rotation, Vector3 position, Vector3 targetPosition, float fraction)
        {
            Vector3 direction = targetPosition - position;
            direction.y = 0f;
            if (direction.sqrMagnitude > MIN_TURN_DISTANCE_SQUARED)
            {
                rotation = Quaternion.Slerp(rotation, Quaternion.LookRotation(direction), fraction);
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

        private static void UpdateAttack(ref EnemyEntity enemy, in EntityIdentifier identifier, bool hasPlayer, in EntityIdentifier playerIdentifier, Vector3 playerPosition, float playerRadius, float deltaTime, List<EnemyStateChangedRecord> stateRecords, Queue<ProjectileSpawnRequest> projectileSpawnRequests, Queue<PlayerDamageRequest> playerDamageRequests)
        {
            if (enemy.hasAttackTarget && !enemy.isAttackCommitted && hasPlayer)
            {
                TurnToward(ref enemy.currentRotation, enemy.currentPosition, playerPosition, deltaTime * enemy.behavior.attackRotationSpeed);
            }

            ref readonly EnemyAttackComponent attack = ref enemy.attacks[enemy.attackIndex];
            if (!enemy.isAttackCommitted && enemy.stateElapsedTicks >= attack.commitTicks)
            {
                enemy.isAttackCommitted = true;
                int missileShotCount = 0;
                CommitAttack(in attack, enemy.currentPosition, enemy.currentRotation, enemy.attackDamage, ref missileShotCount, hasPlayer, in playerIdentifier, playerPosition, playerRadius, projectileSpawnRequests, playerDamageRequests);
            }

            if (enemy.stateElapsedTicks >= attack.durationTicks)
            {
                ChangeState(ref enemy, in identifier, EnemyState.Idle, stateRecords);
            }
        }

        // 공격을 무작위로 하나 골라 공격 상태에 들어간다. 공격이 없으면 Idle로 돌아간다.
        private static void BeginAttack(ref EnemyEntity enemy, in EntityIdentifier identifier, System.Random random, List<EnemyStateChangedRecord> stateRecords)
        {
            if (enemy.attacks.Length == 0)
            {
                Debug.LogWarning($"EnemyBehaviorSystem >> 적 종류 {enemy.kind}에 공격이 없어 공격하지 않았다.");
                ChangeState(ref enemy, in identifier, EnemyState.Idle, stateRecords);
                return;
            }

            enemy.attackIndex = random.Next(0, enemy.attacks.Length);
            enemy.isAttackCommitted = false;
            enemy.hasAttackTarget = true;
            ChangeState(ref enemy, in identifier, EnemyState.Attack, stateRecords);
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

        private static bool IsWithin(Vector3 from, Vector3 to, float distance)
        {
            return (to - from).sqrMagnitude <= distance * distance;
        }
    }
}
