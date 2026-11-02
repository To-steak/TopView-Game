using System.Collections.Generic;
using TopView.Components;
using TopView.Entities;
using TopView.Records;
using TopView.Requests;
using UnityEngine;

namespace TopView.Systems
{
    // 공격 상태의 적을 플레이어 쪽으로 돌리고, 시간이 되면 공격을 내보내며, 공격이 끝나면 Idle로 돌려보낸다.
    public static class EnemyAttackStateSystem
    {
        /// <summary>
        /// 공격 상태의 적을 처리한다. 이번 틱에 막 공격 상태에 들어온 적은 다음 틱부터 처리한다. EnemyBehaviorSystem 다음에 부른다.
        /// </summary>
        /// <param name="masks">칸별 컴포넌트 표시</param>
        /// <param name="generations">칸이 채워진 횟수</param>
        /// <param name="combats">레벨과 공격력</param>
        /// <param name="positions">위치</param>
        /// <param name="rotations">회전</param>
        /// <param name="attackLists">공격 목록</param>
        /// <param name="behaviors">적 행동 값</param>
        /// <param name="actionStates">적 행동 상태와 공격 진행</param>
        /// <param name="agents">목적지와 경로</param>
        /// <param name="playerMovements">플레이어 이동 값. 플레이어를 찾는 데 쓴다</param>
        /// <param name="stateRecords">적 상태 변경 기록</param>
        /// <param name="projectileSpawnRequests">투사체 생성 요청 큐</param>
        /// <param name="meleeSpawnRequests">근접 판정 생성 요청 큐</param>
        public static void Tick(ComponentMask[] masks, int[] generations, CombatComponent[] combats, PositionComponent[] positions, RotationComponent[] rotations, AttackListComponent[] attackLists, EnemyBehaviorComponent[] behaviors, EnemyActionStateComponent[] actionStates, NavigationAgentComponent[] agents, PlayerMovementComponent[] playerMovements, List<EnemyStateChangedRecord> stateRecords, Queue<ProjectileSpawnRequest> projectileSpawnRequests, Queue<MeleeSpawnRequest> meleeSpawnRequests)
        {
            bool hasPlayer = TargetingSystem.TryGetPlayerTarget(masks, positions, playerMovements, out Vector3 playerPosition, out float _);
            float deltaTime = Time.fixedDeltaTime;

            for (int index = 0; index < masks.Length; index++)
            {
                if ((masks[index] & ComponentMask.EnemyArchetype) != ComponentMask.EnemyArchetype)
                {
                    continue;
                }

                ref EnemyActionStateComponent actionState = ref actionStates[index];
                // 타이머가 이번 틱에 이미 늘렸으므로 시간이 0이면 EnemyBehaviorSystem이 방금 공격 상태로 보낸 적이다
                if (actionState.state != EnemyState.Attack || actionState.elapsedTicks == 0)
                {
                    continue;
                }

                ref RotationComponent rotation = ref rotations[index];
                Vector3 position = positions[index].current;

                // 공격이 나가기 전까지 플레이어 쪽으로 돈다
                if (!actionState.isAttackCommitted && hasPlayer)
                {
                    TargetingSystem.RotateToward(ref rotation.current, position, playerPosition, behaviors[index].angularSpeed * deltaTime);
                }

                EnemyAttackComponent[] attacks = attackLists[index].attacks;
                ref readonly EnemyAttackComponent attack = ref attacks[actionState.attackIndex];
                EntityIdentifier identifier = new EntityIdentifier { index = index, generation = generations[index] };
                if (!actionState.isAttackCommitted && actionState.elapsedTicks >= attack.startTicks)
                {
                    actionState.isAttackCommitted = true;
                    int missileShotCount = 0;
                    EnemyAttackSystem.CommitAttack(in attack, position, rotation.current, combats[index].attackDamage, ref missileShotCount, in identifier, projectileSpawnRequests, meleeSpawnRequests);
                }

                if (EnemyAttackSystem.IsAttackFinished(in attack, actionState.elapsedTicks, actionState.isAttackCommitted))
                {
                    actionState.attackCooldownRemainingTicks[actionState.attackIndex] = attack.cooldownTicks;
                    EnemyBehaviorSystem.ChangeState(ref actionState, ref agents[index], attacks, in identifier, EnemyState.Idle, stateRecords);
                }
            }
        }
    }
}
