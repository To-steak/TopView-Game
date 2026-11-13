using System.Collections.Generic;
using TopView.Components;
using TopView.Entities;
using TopView.Records;
using TopView.Requests;
using TopView.Utilities;
using UnityEngine;

namespace TopView.Systems
{
    // 공격 상태의 적을 첫 발이 나가기 전까지 플레이어 쪽으로 돌리고, 시점마다 공격을 내보내며, 공격이 끝나면 Idle을 요청한다. 투사체는 발마다 플레이어 발밑을 겨냥한다.
    public static class EnemyAttackStateSystem
    {
        // 처리할 적이 가진 컴포넌트
        private const ComponentMask ENEMY_MASK = ComponentMask.Position | ComponentMask.Rotation | ComponentMask.NavigationMovement | ComponentMask.AttackList
            | ComponentMask.Combat | ComponentMask.EnemyActionState;

        // 발사 위치와 플레이어 발밑이 이보다 가까우면 겨냥할 방향을 정할 수 없어 정면으로 쏜다(유닛)
        private const float MIN_AIM_DISTANCE = 0.001f;

        /// <summary>
        /// 공격 상태의 적을 처리한다. 이번 틱에 막 공격 상태에 들어온 적은 다음 틱부터 처리한다. EnemyBehaviorSystem의 요청을 처리한 뒤, EnemyStateChangeSystem 앞에 부른다.
        /// </summary>
        /// <param name="storage">엔티티 저장소. 적(Position, Rotation, NavigationMovement, AttackList, Combat, EnemyActionState)만 처리하고, 회전과 공격 진행을 고친다</param>
        /// <param name="playerLocation">바라보고 투사체로 겨냥할 플레이어의 위치</param>
        /// <param name="stateChangeRequests">적 상태 변경 요청 큐</param>
        /// <param name="projectileSpawnRequests">직선 투사체 생성 요청 큐</param>
        /// <param name="meleeSpawnRequests">근접 판정 생성 요청 큐</param>
        /// <param name="attackCommittedRecords">이번 프레임에 적이 공격을 내보낸 기록</param>
        public static void Tick(EntityStorage storage, in PlayerLocationComponent playerLocation, Queue<EnemyStateChangeRequest> stateChangeRequests, Queue<StraightProjectileSpawnRequest> projectileSpawnRequests, Queue<MeleeSpawnRequest> meleeSpawnRequests, List<EnemyAttackCommittedRecord> attackCommittedRecords)
        {
            bool hasPlayer = playerLocation.hasPlayer;
            Vector3 playerPosition = playerLocation.position;
            float deltaTime = Time.fixedDeltaTime;

            for (int index = 0; index < storage.entities.Length; index++)
            {
                ref Entity entity = ref storage.entities[index];
                if ((entity.mask & ENEMY_MASK) != ENEMY_MASK)
                {
                    continue;
                }

                ref EnemyActionStateComponent actionState = ref entity.enemyActionState;
                // 타이머가 이번 틱에 이미 늘렸으므로 시간이 0이면 EnemyBehaviorSystem이 방금 공격 상태로 보낸 적이다
                if (actionState.state != EnemyState.Attack || actionState.elapsedTicks == 0)
                {
                    continue;
                }

                ref RotationComponent rotation = ref entity.rotation;
                Vector3 position = entity.position.current;

                // 첫 발이 나가기 전까지 플레이어 쪽으로 돈다
                if (actionState.committedShotCount == 0 && hasPlayer)
                {
                    TargetingUtility.RotateToward(ref rotation.current, position, playerPosition, entity.navigationMovement.angularSpeed * deltaTime);
                }

                EnemyAttackComponent[] attacks = entity.attackList.attacks;
                ref readonly EnemyAttackComponent attack = ref attacks[actionState.attackIndex];
                EntityIdentifier identifier = new EntityIdentifier { index = index, generation = entity.generation };
                float damage = entity.combat.attackDamage * attack.damageMultiplier;
                if (attack.kind == EnemyAttackKind.Body)
                {
                    if (AttackUtility.IsShotDue(in attack.melee, actionState.committedShotCount, actionState.elapsedTicks))
                    {
                        actionState.committedShotCount++;
                        meleeSpawnRequests.Enqueue(AttackUtility.CreateSpawnRequest(in attack.melee, position, rotation.current, in identifier, Faction.Enemy, damage));
                        attackCommittedRecords.Add(new EnemyAttackCommittedRecord { enemy = identifier, attackIndex = actionState.attackIndex });
                    }
                }
                else
                {
                    // 시점이 지난 발을 차례로 내보낸다. 같은 틱에 시점이 겹친 발은 함께 나간다
                    while (AttackUtility.IsShotDue(in attack.straight, actionState.committedShotCount, actionState.elapsedTicks))
                    {
                        Vector3 shotPosition = AttackUtility.GetShotPosition(in attack.straight.shots[actionState.committedShotCount], position, rotation.current);
                        Quaternion shotRotation = GetAimRotation(shotPosition, rotation.current, hasPlayer, playerPosition);
                        projectileSpawnRequests.Enqueue(AttackUtility.CreateSpawnRequest(in attack.straight, in attack.projectileView, shotPosition, shotRotation, Faction.Enemy, damage));
                        attackCommittedRecords.Add(new EnemyAttackCommittedRecord { enemy = identifier, attackIndex = actionState.attackIndex });
                        actionState.committedShotCount++;
                    }
                }

                if (EnemyAttackUtility.IsAttackFinished(in attack, actionState.elapsedTicks, actionState.committedShotCount))
                {
                    actionState.attackCooldownRemainingTicks[actionState.attackIndex] = attack.cooldownTicks;
                    stateChangeRequests.Enqueue(new EnemyStateChangeRequest { target = identifier, state = EnemyState.Idle });
                }
            }
        }

        // 발이 생기는 위치에서 플레이어 발밑을 향하는 회전을 돌려준다. 플레이어가 없거나 발사 위치와 겹치면 적의 회전을 그대로 돌려준다.
        private static Quaternion GetAimRotation(Vector3 shotPosition, Quaternion rotation, bool hasPlayer, Vector3 playerPosition)
        {
            Vector3 toPlayer = playerPosition - shotPosition;
            if (hasPlayer && toPlayer.sqrMagnitude > MIN_AIM_DISTANCE * MIN_AIM_DISTANCE)
            {
                return Quaternion.LookRotation(toPlayer);
            }

            return rotation;
        }
    }
}
