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
        /// 공격 하나를 내보낸다. Body는 한 틱 동안 판정하는 근접 판정 생성 요청을 넣고, 나머지는 투사체 생성 요청을 넣는다.
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
                    durationTicks = 1
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
