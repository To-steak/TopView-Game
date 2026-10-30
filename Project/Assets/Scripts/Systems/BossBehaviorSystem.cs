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
        /// 살아 있는 보스마다 상태 시간을 1틱 늘리고, 상태별로 도발·대기·공격·소멸을 처리한다. 사망 후 사라질 때 게임오버를 요청한다.
        /// </summary>
        /// <param name="bosses">보스 저장소</param>
        /// <param name="players">플레이어 저장소. 바라보고 공격할 대상이다</param>
        /// <param name="random">World의 난수 생성기</param>
        /// <param name="stateRecords">보스 상태 변경 기록</param>
        /// <param name="projectileSpawnRequests">투사체 생성 요청 큐</param>
        /// <param name="playerDamageRequests">플레이어 피해 요청 큐</param>
        /// <param name="gameOverRequests">게임오버 요청 큐</param>
        public static void Tick(BossStorage bosses, PlayerStorage players, System.Random random, List<BossStateChangedRecord> stateRecords, Queue<ProjectileSpawnRequest> projectileSpawnRequests, Queue<PlayerDamageRequest> playerDamageRequests, Queue<GameOverRequest> gameOverRequests)
        {
            bool hasPlayer = EnemyBehaviorSystem.TryGetPlayerTarget(players, out EntityIdentifier playerIdentifier, out Vector3 playerPosition, out float playerRadius);
            float deltaTime = Time.fixedDeltaTime;

            for (int bossIndex = 0; bossIndex < bosses.entities.Length; bossIndex++)
            {
                ref BossEntity boss = ref bosses.entities[bossIndex];
                if (!boss.isAlive)
                {
                    continue;
                }

                EntityIdentifier identifier = new EntityIdentifier { index = bossIndex, generation = boss.generation };
                boss.stateElapsedTicks++;

                switch (boss.state)
                {
                    case BossState.Taunt:
                        if (boss.stateElapsedTicks >= boss.behavior.tauntTicks)
                        {
                            ChangeState(ref boss, in identifier, BossState.Idle, stateRecords);
                        }
                        break;

                    case BossState.Idle:
                        if (boss.stateElapsedTicks >= boss.behavior.attackEntryTicks)
                        {
                            BeginAttack(ref boss, in identifier, random, stateRecords);
                            break;
                        }
                        if (hasPlayer)
                        {
                            EnemyBehaviorSystem.TurnToward(ref boss.currentRotation, boss.currentPosition, playerPosition, deltaTime * boss.behavior.rotationSpeed);
                        }
                        break;

                    case BossState.Attack:
                        ref readonly EnemyAttackComponent attack = ref boss.attacks[boss.attackIndex];
                        if (!boss.isAttackCommitted && boss.stateElapsedTicks >= attack.commitTicks)
                        {
                            boss.isAttackCommitted = true;
                            EnemyBehaviorSystem.CommitAttack(in attack, boss.currentPosition, boss.currentRotation, boss.attackDamage, ref boss.missileShotCount, hasPlayer, in playerIdentifier, playerPosition, playerRadius, projectileSpawnRequests, playerDamageRequests);
                        }
                        if (boss.stateElapsedTicks >= attack.durationTicks)
                        {
                            ChangeState(ref boss, in identifier, BossState.Idle, stateRecords);
                        }
                        break;

                    case BossState.Die:
                        if (boss.stateElapsedTicks >= boss.behavior.dieTicks)
                        {
                            boss.isAlive = false;
                            bosses.freeIndices.Add(bossIndex);
                            gameOverRequests.Enqueue(new GameOverRequest());
                        }
                        break;
                }
            }
        }

        /// <summary>
        /// 새 상태에 들어간다. Die 상태에서는 바꾸지 않는다. Attack 상태는 공격 번호를 정한 뒤 들어간다.
        /// </summary>
        /// <param name="boss">보스</param>
        /// <param name="identifier">보스를 가리키는 값</param>
        /// <param name="newState">새 상태</param>
        /// <param name="stateRecords">보스 상태 변경 기록</param>
        public static void ChangeState(ref BossEntity boss, in EntityIdentifier identifier, BossState newState, List<BossStateChangedRecord> stateRecords)
        {
            if (boss.state == BossState.Die)
            {
                return;
            }

            stateRecords.Add(new BossStateChangedRecord
            {
                boss = identifier,
                previousState = boss.state,
                state = newState,
                attackKind = newState == BossState.Attack ? boss.attacks[boss.attackIndex].kind : default
            });

            boss.state = newState;
            boss.stateElapsedTicks = 0;
        }

        // 공격을 무작위로 하나 골라 공격 상태에 들어간다. 공격이 없으면 대기 시간을 처음부터 다시 센다.
        private static void BeginAttack(ref BossEntity boss, in EntityIdentifier identifier, System.Random random, List<BossStateChangedRecord> stateRecords)
        {
            if (boss.attacks.Length == 0)
            {
                Debug.LogWarning("BossBehaviorSystem >> 보스에 공격이 없어 공격하지 않았다.");
                boss.stateElapsedTicks = 0;
                return;
            }

            boss.attackIndex = random.Next(0, boss.attacks.Length);
            boss.isAttackCommitted = false;
            ChangeState(ref boss, in identifier, BossState.Attack, stateRecords);
        }
    }
}
