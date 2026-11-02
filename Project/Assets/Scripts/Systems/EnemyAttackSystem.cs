using System.Collections.Generic;
using TopView.Components;
using TopView.Entities;
using TopView.Requests;
using UnityEngine;

namespace TopView.Systems
{
    // 적·보스가 함께 쓰는 공격 내보내기 도구.
    public static class EnemyAttackSystem
    {
        /// <summary>
        /// 공격 하나를 내보낸다. Body는 endTicks까지 이어지는 근접 판정 생성 요청을 넣고, 나머지는 투사체 생성 요청을 넣는다.
        /// </summary>
        /// <param name="attack">공격 값</param>
        /// <param name="position">공격하는 쪽의 위치(월드)</param>
        /// <param name="rotation">공격하는 쪽의 회전(월드)</param>
        /// <param name="attackDamage">공격하는 쪽의 공격력</param>
        /// <param name="missileShotCount">BossMissile을 쏜 횟수. 짝수·홀수로 발사 위치를 고르고 1 늘린다</param>
        /// <param name="owner">공격하는 엔티티</param>
        /// <param name="projectileSpawnRequests">투사체 생성 요청 큐</param>
        /// <param name="meleeSpawnRequests">근접 판정 생성 요청 큐</param>
        public static void CommitAttack(in EnemyAttackComponent attack, Vector3 position, Quaternion rotation, float attackDamage, ref int missileShotCount, in EntityReference owner, Queue<ProjectileSpawnRequest> projectileSpawnRequests, Queue<MeleeSpawnRequest> meleeSpawnRequests)
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

        /// <summary>
        /// 공격 상태가 끝났는지 돌려준다. Body는 후딜레이까지 기다리고, 투사체 공격은 투사체를 내보내면 바로 끝난다.
        /// </summary>
        /// <param name="attack">공격 값</param>
        /// <param name="stateElapsedTicks">공격 상태에 들어간 뒤 지난 시간(틱)</param>
        /// <param name="isAttackCommitted">이번 공격 상태에서 공격을 내보냈는지</param>
        /// <returns>공격 상태가 끝났으면 true</returns>
        public static bool IsAttackFinished(in EnemyAttackComponent attack, int stateElapsedTicks, bool isAttackCommitted)
        {
            return attack.kind == EnemyAttackKind.Body
                ? stateElapsedTicks >= attack.durationTicks
                : isAttackCommitted;
        }
    }
}
