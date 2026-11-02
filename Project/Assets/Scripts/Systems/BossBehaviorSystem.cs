using System.Collections.Generic;
using TopView.Components;
using TopView.Entities;
using TopView.Records;
using TopView.Requests;
using UnityEngine;

namespace TopView.Systems
{
    // 보스의 행동 상태를 상태 타이머로 바꾸고, 플레이어 쪽으로 돌며 공격을 내보낸다.
    public static class BossBehaviorSystem
    {
        /// <summary>
        /// 보스마다 체력이 0이면 사망 상태로 바꾸고, 상태 시간을 1틱 늘린 뒤 상태별로 도발·대기·공격·소멸을 처리한다. 사망 후 사라질 때 게임오버를 요청한다.
        /// </summary>
        /// <param name="masks">칸별 컴포넌트 표시</param>
        /// <param name="generations">칸이 채워진 횟수</param>
        /// <param name="healths">체력</param>
        /// <param name="combats">레벨과 공격력</param>
        /// <param name="positions">위치</param>
        /// <param name="rotations">회전</param>
        /// <param name="attackLists">공격 목록</param>
        /// <param name="behaviors">보스 행동 값</param>
        /// <param name="actionStates">보스 행동 상태와 공격 진행</param>
        /// <param name="freeBossIndices">보스 구간의 빈 칸 번호 목록</param>
        /// <param name="players">플레이어 저장소. 바라보고 공격할 대상이다</param>
        /// <param name="random">World의 난수 생성기</param>
        /// <param name="stateRecords">보스 상태 변경 기록</param>
        /// <param name="projectileSpawnRequests">투사체 생성 요청 큐</param>
        /// <param name="meleeSpawnRequests">근접 판정 생성 요청 큐</param>
        /// <param name="gameOverRequests">게임오버 요청 큐</param>
        public static void Tick(ComponentMask[] masks, int[] generations, HealthComponent[] healths, CombatComponent[] combats, PositionComponent[] positions, RotationComponent[] rotations, AttackListComponent[] attackLists, BossBehaviorComponent[] behaviors, BossActionStateComponent[] actionStates, List<int> freeBossIndices, PlayerStorage players, System.Random random, List<BossStateChangedRecord> stateRecords, Queue<ProjectileSpawnRequest> projectileSpawnRequests, Queue<MeleeSpawnRequest> meleeSpawnRequests, Queue<GameOverRequest> gameOverRequests)
        {
            bool hasPlayer = TargetingSystem.TryGetPlayerTarget(players, out Vector3 playerPosition, out float _);
            float deltaTime = Time.fixedDeltaTime;

            for (int index = 0; index < masks.Length; index++)
            {
                if ((masks[index] & ComponentMask.BossArchetype) != ComponentMask.BossArchetype)
                {
                    continue;
                }

                ref BossActionStateComponent actionState = ref actionStates[index];
                ref readonly BossBehaviorComponent behavior = ref behaviors[index];
                EnemyAttackComponent[] attacks = attackLists[index].attacks;

                EntityIdentifier identifier = new EntityIdentifier { index = index, generation = generations[index] };
                if (healths[index].current <= 0f && actionState.state != BossState.Die)
                {
                    ChangeState(ref actionState, attacks, in identifier, BossState.Die, stateRecords);
                }

                actionState.elapsedTicks++;

                switch (actionState.state)
                {
                    case BossState.Taunt:
                        if (actionState.elapsedTicks >= behavior.tauntTicks)
                        {
                            ChangeState(ref actionState, attacks, in identifier, BossState.Idle, stateRecords);
                        }
                        break;

                    case BossState.Idle:
                        if (actionState.elapsedTicks >= behavior.attackEntryTicks)
                        {
                            BeginAttack(ref actionState, attacks, in identifier, random, stateRecords);
                            break;
                        }
                        if (hasPlayer)
                        {
                            TargetingSystem.TurnToward(ref rotations[index].current, positions[index].current, playerPosition, deltaTime * behavior.rotationSpeed);
                        }
                        break;

                    case BossState.Attack:
                        ref readonly EnemyAttackComponent attack = ref attacks[actionState.attackIndex];
                        if (!actionState.isAttackCommitted && actionState.elapsedTicks >= attack.startTicks)
                        {
                            actionState.isAttackCommitted = true;
                            EntityReference owner = new EntityReference { kind = EntityKind.Shared, identifier = identifier };
                            EnemyAttackSystem.CommitAttack(in attack, positions[index].current, rotations[index].current, combats[index].attackDamage, ref actionState.missileShotCount, in owner, projectileSpawnRequests, meleeSpawnRequests);
                        }
                        if (EnemyAttackSystem.IsAttackFinished(in attack, actionState.elapsedTicks, actionState.isAttackCommitted))
                        {
                            ChangeState(ref actionState, attacks, in identifier, BossState.Idle, stateRecords);
                        }
                        break;

                    case BossState.Die:
                        if (actionState.elapsedTicks >= behavior.dieTicks)
                        {
                            SpawnSystem.ReleaseSlot(masks, freeBossIndices, index);
                            gameOverRequests.Enqueue(new GameOverRequest());
                        }
                        break;
                }
            }
        }

        // 새 상태에 들어간다. Die 상태에서는 바꾸지 않는다. Attack 상태는 공격 번호를 정한 뒤 들어간다.
        private static void ChangeState(ref BossActionStateComponent actionState, EnemyAttackComponent[] attacks, in EntityIdentifier identifier, BossState newState, List<BossStateChangedRecord> stateRecords)
        {
            if (actionState.state == BossState.Die)
            {
                return;
            }

            stateRecords.Add(new BossStateChangedRecord
            {
                boss = identifier,
                previousState = actionState.state,
                state = newState,
                attackKind = newState == BossState.Attack ? attacks[actionState.attackIndex].kind : default
            });

            actionState.state = newState;
            actionState.elapsedTicks = 0;
        }

        // 공격을 무작위로 하나 골라 공격 상태에 들어간다. 공격이 없으면 대기 시간을 처음부터 다시 센다.
        private static void BeginAttack(ref BossActionStateComponent actionState, EnemyAttackComponent[] attacks, in EntityIdentifier identifier, System.Random random, List<BossStateChangedRecord> stateRecords)
        {
            if (attacks.Length == 0)
            {
                Debug.LogWarning("BossBehaviorSystem >> 보스에 공격이 없어 공격하지 않았다.");
                actionState.elapsedTicks = 0;
                return;
            }

            actionState.attackIndex = random.Next(0, attacks.Length);
            actionState.isAttackCommitted = false;
            ChangeState(ref actionState, attacks, in identifier, BossState.Attack, stateRecords);
        }
    }
}
