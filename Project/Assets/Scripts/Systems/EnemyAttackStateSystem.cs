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
        /// <param name="enemies">적 저장소</param>
        /// <param name="players">플레이어 저장소. 바라볼 위치를 읽는다</param>
        /// <param name="stateRecords">적 상태 변경 기록</param>
        /// <param name="projectileSpawnRequests">투사체 생성 요청 큐</param>
        /// <param name="meleeSpawnRequests">근접 판정 생성 요청 큐</param>
        public static void Tick(EnemyStorage enemies, PlayerStorage players, List<EnemyStateChangedRecord> stateRecords, Queue<ProjectileSpawnRequest> projectileSpawnRequests, Queue<MeleeSpawnRequest> meleeSpawnRequests)
        {
            bool hasPlayer = TargetingSystem.TryGetPlayerTarget(players, out Vector3 playerPosition, out float _);
            float deltaTime = Time.fixedDeltaTime;

            for (int index = 0; index < enemies.entities.Length; index++)
            {
                ref EnemyEntity entity = ref enemies.entities[index];
                if (!entity.isAlive)
                {
                    continue;
                }

                ref EnemyActionStateComponent actionState = ref entity.actionState;
                // 타이머가 이번 틱에 이미 늘렸으므로 시간이 0이면 EnemyBehaviorSystem이 방금 공격 상태로 보낸 적이다
                if (actionState.state != EnemyState.Attack || actionState.elapsedTicks == 0)
                {
                    continue;
                }

                ref RotationComponent rotation = ref entity.rotation;
                Vector3 position = entity.position.current;

                // 공격이 나가기 전까지 플레이어 쪽으로 돈다
                if (!actionState.isAttackCommitted && hasPlayer)
                {
                    TargetingSystem.RotateToward(ref rotation.current, position, playerPosition, entity.behavior.angularSpeed * deltaTime);
                }

                EnemyAttackComponent[] attacks = entity.attackList.attacks;
                ref readonly EnemyAttackComponent attack = ref attacks[actionState.attackIndex];
                EntityIdentifier identifier = new EntityIdentifier { index = index, generation = entity.generation };
                if (!actionState.isAttackCommitted && actionState.elapsedTicks >= attack.startTicks)
                {
                    actionState.isAttackCommitted = true;
                    EnemyAttackSystem.CommitAttack(in attack, position, rotation.current, entity.combat.attackDamage, ref actionState.missileShotCount, in identifier, projectileSpawnRequests, meleeSpawnRequests);
                }

                if (EnemyAttackSystem.IsAttackFinished(in attack, actionState.elapsedTicks, actionState.isAttackCommitted))
                {
                    actionState.attackCooldownRemainingTicks[actionState.attackIndex] = attack.cooldownTicks;
                    EnemyBehaviorSystem.ChangeState(ref actionState, ref entity.agent, attacks, in identifier, EnemyState.Idle, stateRecords);
                }
            }
        }
    }
}
