using System.Collections.Generic;
using TopView.Components;
using TopView.Entities;
using TopView.Requests;
using TopView.Utilities;
using UnityEngine;

namespace TopView.Systems
{
    // 공격 상태의 적을 플레이어 쪽으로 돌리고, 시간이 되면 공격을 내보내며, 공격이 끝나면 Idle을 요청한다.
    public static class EnemyAttackStateSystem
    {
        /// <summary>
        /// 공격 상태의 적을 처리한다. 이번 틱에 막 공격 상태에 들어온 적은 다음 틱부터 처리한다. EnemyBehaviorSystem의 요청을 처리한 뒤, EnemyStateChangeSystem 앞에 부른다.
        /// </summary>
        /// <param name="enemies">적 저장소</param>
        /// <param name="playerLocation">바라볼 플레이어의 위치</param>
        /// <param name="stateChangeRequests">적 상태 변경 요청 큐</param>
        /// <param name="projectileSpawnRequests">투사체 생성 요청 큐</param>
        /// <param name="meleeSpawnRequests">근접 판정 생성 요청 큐</param>
        public static void Tick(EnemyStorage enemies, in PlayerLocationComponent playerLocation, Queue<EnemyStateChangeRequest> stateChangeRequests, Queue<ProjectileSpawnRequest> projectileSpawnRequests, Queue<MeleeSpawnRequest> meleeSpawnRequests)
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

                // 공격이 나가기 전까지 플레이어 쪽으로 돈다
                if (!actionState.isAttackCommitted && hasPlayer)
                {
                    TargetingUtility.RotateToward(ref rotation.current, position, playerPosition, entity.behavior.angularSpeed * deltaTime);
                }

                EnemyAttackComponent[] attacks = entity.attackList.attacks;
                ref readonly EnemyAttackComponent attack = ref attacks[actionState.attackIndex];
                EntityIdentifier identifier = new EntityIdentifier { index = index, generation = entity.generation };
                if (!actionState.isAttackCommitted && actionState.elapsedTicks >= attack.startTicks)
                {
                    actionState.isAttackCommitted = true;
                    CommitAttack(in attack, position, rotation.current, entity.combat.attackDamage, ref actionState.missileShotCount, in identifier, projectileSpawnRequests, meleeSpawnRequests);
                }

                if (EnemyAttackUtility.IsAttackFinished(in attack, actionState.elapsedTicks, actionState.isAttackCommitted))
                {
                    actionState.attackCooldownRemainingTicks[actionState.attackIndex] = attack.cooldownTicks;
                    stateChangeRequests.Enqueue(new EnemyStateChangeRequest { target = identifier, state = EnemyState.Idle });
                }
            }
        }

        // 공격 하나를 내보낸다. Body는 endTicks까지 이어지는 근접 판정 생성 요청을 넣고, 나머지는 투사체 생성 요청을 넣는다. BossMissile은 쏜 횟수의 짝수·홀수로 발사 위치를 고른다.
        private static void CommitAttack(in EnemyAttackComponent attack, Vector3 position, Quaternion rotation, float attackDamage, ref int missileShotCount, in EntityIdentifier owner, Queue<ProjectileSpawnRequest> projectileSpawnRequests, Queue<MeleeSpawnRequest> meleeSpawnRequests)
        {
            float damage = attackDamage * attack.damageMultiplier;

            if (attack.kind == EnemyAttackKind.Body)
            {
                meleeSpawnRequests.Enqueue(new MeleeSpawnRequest
                {
                    owner = owner,
                    faction = Faction.Enemy,
                    center = position + rotation * Vector3.forward * attack.bodyOffset,
                    radius = attack.bodyRadius,
                    damage = damage,
                    durationTicks = attack.endTicks - attack.startTicks
                });
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
                faction = Faction.Enemy,
                damage = damage,
                position = position + rotation * spawnOffset,
                rotation = rotation,
                velocity = rotation * Vector3.forward * attack.projectile.speed
            });
        }
    }
}
