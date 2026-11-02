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
        /// 살아 있는 보스마다 체력이 0이면 사망 상태로 바꾸고, 상태 시간을 1틱 늘린 뒤 상태별로 도발·대기·공격·소멸을 처리한다. 사망 후 사라질 때 게임오버를 요청한다.
        /// </summary>
        /// <param name="bosses">보스 저장소</param>
        /// <param name="players">플레이어 저장소. 바라보고 공격할 대상이다</param>
        /// <param name="random">World의 난수 생성기</param>
        /// <param name="stateRecords">보스 상태 변경 기록</param>
        /// <param name="projectileSpawnRequests">투사체 생성 요청 큐</param>
        /// <param name="meleeSpawnRequests">근접 판정 생성 요청 큐</param>
        /// <param name="gameOverRequests">게임오버 요청 큐</param>
        public static void Tick(BossStorage bosses, PlayerStorage players, System.Random random, List<BossStateChangedRecord> stateRecords, Queue<ProjectileSpawnRequest> projectileSpawnRequests, Queue<MeleeSpawnRequest> meleeSpawnRequests, Queue<GameOverRequest> gameOverRequests)
        {
            bool hasPlayer = TargetingSystem.TryGetPlayerTarget(players, out Vector3 playerPosition, out float _);
            float deltaTime = Time.fixedDeltaTime;

            for (int bossIndex = 0; bossIndex < bosses.entities.Length; bossIndex++)
            {
                ref BossEntity boss = ref bosses.entities[bossIndex];
                if (!boss.isAlive)
                {
                    continue;
                }

                EntityIdentifier identifier = new EntityIdentifier { index = bossIndex, generation = boss.generation };
                if (boss.health.current <= 0f && boss.actionState.state != BossState.Die)
                {
                    ChangeState(ref boss, in identifier, BossState.Die, stateRecords);
                }

                boss.actionState.elapsedTicks++;

                switch (boss.actionState.state)
                {
                    case BossState.Taunt:
                        if (boss.actionState.elapsedTicks >= boss.behavior.tauntTicks)
                        {
                            ChangeState(ref boss, in identifier, BossState.Idle, stateRecords);
                        }
                        break;

                    case BossState.Idle:
                        if (boss.actionState.elapsedTicks >= boss.behavior.attackEntryTicks)
                        {
                            BeginAttack(ref boss, in identifier, random, stateRecords);
                            break;
                        }
                        if (hasPlayer)
                        {
                            TargetingSystem.TurnToward(ref boss.rotation.current, boss.position.current, playerPosition, deltaTime * boss.behavior.rotationSpeed);
                        }
                        break;

                    case BossState.Attack:
                        ref readonly EnemyAttackComponent attack = ref boss.attacks[boss.actionState.attackIndex];
                        if (!boss.actionState.isAttackCommitted && boss.actionState.elapsedTicks >= attack.startTicks)
                        {
                            boss.actionState.isAttackCommitted = true;
                            EntityReference owner = new EntityReference { kind = EntityKind.Boss, identifier = identifier };
                            EnemyAttackSystem.CommitAttack(in attack, boss.position.current, boss.rotation.current, boss.combat.attackDamage, ref boss.actionState.missileShotCount, in owner, projectileSpawnRequests, meleeSpawnRequests);
                        }
                        if (EnemyAttackSystem.IsAttackFinished(in attack, boss.actionState.elapsedTicks, boss.actionState.isAttackCommitted))
                        {
                            ChangeState(ref boss, in identifier, BossState.Idle, stateRecords);
                        }
                        break;

                    case BossState.Die:
                        if (boss.actionState.elapsedTicks >= boss.behavior.dieTicks)
                        {
                            boss.isAlive = false;
                            bosses.freeIndices.Add(bossIndex);
                            gameOverRequests.Enqueue(new GameOverRequest());
                        }
                        break;
                }
            }
        }

        // 새 상태에 들어간다. Die 상태에서는 바꾸지 않는다. Attack 상태는 공격 번호를 정한 뒤 들어간다.
        private static void ChangeState(ref BossEntity boss, in EntityIdentifier identifier, BossState newState, List<BossStateChangedRecord> stateRecords)
        {
            if (boss.actionState.state == BossState.Die)
            {
                return;
            }

            stateRecords.Add(new BossStateChangedRecord
            {
                boss = identifier,
                previousState = boss.actionState.state,
                state = newState,
                attackKind = newState == BossState.Attack ? boss.attacks[boss.actionState.attackIndex].kind : default
            });

            boss.actionState.state = newState;
            boss.actionState.elapsedTicks = 0;
        }

        // 공격을 무작위로 하나 골라 공격 상태에 들어간다. 공격이 없으면 대기 시간을 처음부터 다시 센다.
        private static void BeginAttack(ref BossEntity boss, in EntityIdentifier identifier, System.Random random, List<BossStateChangedRecord> stateRecords)
        {
            if (boss.attacks.Length == 0)
            {
                Debug.LogWarning("BossBehaviorSystem >> 보스에 공격이 없어 공격하지 않았다.");
                boss.actionState.elapsedTicks = 0;
                return;
            }

            boss.actionState.attackIndex = random.Next(0, boss.attacks.Length);
            boss.actionState.isAttackCommitted = false;
            ChangeState(ref boss, in identifier, BossState.Attack, stateRecords);
        }
    }
}
