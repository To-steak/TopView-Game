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
        // 발사 위치와 플레이어 발밑이 이보다 가까우면 겨냥할 방향을 정할 수 없어 정면으로 쏜다(유닛)
        private const float MIN_AIM_DISTANCE = 0.001f;

        /// <summary>
        /// 공격 상태의 적을 처리한다. 이번 틱에 막 공격 상태에 들어온 적은 다음 틱부터 처리한다. EnemyBehaviorSystem의 요청을 처리한 뒤, EnemyStateChangeSystem 앞에 부른다.
        /// </summary>
        /// <param name="enemies">적 저장소</param>
        /// <param name="playerLocation">바라보고 투사체로 겨냥할 플레이어의 위치</param>
        /// <param name="stateChangeRequests">적 상태 변경 요청 큐</param>
        /// <param name="projectileSpawnRequests">투사체 생성 요청 큐</param>
        /// <param name="meleeSpawnRequests">근접 판정 생성 요청 큐</param>
        /// <param name="attackCommittedRecords">이번 프레임에 적이 공격을 내보낸 기록</param>
        public static void Tick(EnemyStorage enemies, in PlayerLocationComponent playerLocation, Queue<EnemyStateChangeRequest> stateChangeRequests, Queue<ProjectileSpawnRequest> projectileSpawnRequests, Queue<MeleeSpawnRequest> meleeSpawnRequests, List<EnemyAttackCommittedRecord> attackCommittedRecords)
        {
            bool hasPlayer = playerLocation.hasPlayer;
            Vector3 playerPosition = playerLocation.position;
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

                // 첫 발이 나가기 전까지 플레이어 쪽으로 돈다
                if (actionState.committedShotCount == 0 && hasPlayer)
                {
                    TargetingUtility.RotateToward(ref rotation.current, position, playerPosition, entity.behavior.angularSpeed * deltaTime);
                }

                EnemyAttackComponent[] attacks = entity.attackList.attacks;
                ref readonly EnemyAttackComponent attack = ref attacks[actionState.attackIndex];
                EntityIdentifier identifier = new EntityIdentifier { index = index, generation = entity.generation };
                float damage = entity.combat.attackDamage * attack.damageMultiplier;
                if (attack.kind == EnemyAttackKind.Body)
                {
                    if (actionState.committedShotCount == 0 && actionState.elapsedTicks >= attack.timing.startTicks)
                    {
                        actionState.committedShotCount = 1;
                        CommitBody(in attack, position, rotation.current, damage, in identifier, meleeSpawnRequests);
                        attackCommittedRecords.Add(new EnemyAttackCommittedRecord { enemy = identifier, attackKind = attack.kind });
                    }
                }
                else
                {
                    // 시점이 지난 발을 차례로 내보낸다. 같은 틱에 시점이 겹친 발은 함께 나간다
                    while (actionState.committedShotCount < attack.shots.Length && actionState.elapsedTicks >= attack.shots[actionState.committedShotCount].startTicks)
                    {
                        CommitShot(in attack, in attack.shots[actionState.committedShotCount], position, rotation.current, hasPlayer, playerPosition, damage, projectileSpawnRequests);
                        attackCommittedRecords.Add(new EnemyAttackCommittedRecord { enemy = identifier, attackKind = attack.kind });
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

        // Body 공격을 내보낸다. timing.endTicks까지 이어지는 근접 판정 생성 요청을 넣는다.
        private static void CommitBody(in EnemyAttackComponent attack, Vector3 position, Quaternion rotation, float damage, in EntityIdentifier owner, Queue<MeleeSpawnRequest> meleeSpawnRequests)
        {
            meleeSpawnRequests.Enqueue(new MeleeSpawnRequest
            {
                owner = owner,
                faction = Faction.Enemy,
                center = position + rotation * Vector3.forward * attack.meleeShape.offset,
                radius = attack.meleeShape.radius,
                damage = damage,
                durationTicks = attack.timing.endTicks - attack.timing.startTicks
            });
        }

        // 투사체 공격의 한 발을 그 발의 위치에서 플레이어 발밑을 향해 내보낸다. 플레이어가 없거나 발사 위치와 겹치면 적의 정면으로 내보낸다.
        private static void CommitShot(in EnemyAttackComponent attack, in ProjectileShotComponent shot, Vector3 position, Quaternion rotation, bool hasPlayer, Vector3 playerPosition, float damage, Queue<ProjectileSpawnRequest> projectileSpawnRequests)
        {
            Vector3 spawnPosition = position + rotation * shot.spawnOffset;
            Quaternion shotRotation = rotation;
            Vector3 toPlayer = playerPosition - spawnPosition;
            if (hasPlayer && toPlayer.sqrMagnitude > MIN_AIM_DISTANCE * MIN_AIM_DISTANCE)
            {
                shotRotation = Quaternion.LookRotation(toPlayer);
            }

            projectileSpawnRequests.Enqueue(new ProjectileSpawnRequest
            {
                projectile = attack.projectile,
                faction = Faction.Enemy,
                damage = damage,
                position = spawnPosition,
                rotation = shotRotation,
                velocity = shotRotation * Vector3.forward * attack.projectile.speed
            });
        }
    }
}
