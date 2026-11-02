using System.Collections.Generic;
using TopView.Components;
using TopView.Entities;
using TopView.Requests;
using UnityEngine;

namespace TopView.Systems
{
    // 피해 요청을 플레이어·적·보스의 체력에 반영하고, 적·보스를 처치하면 점수를 더한다. 사망 상태로 바꾸는 일은 각 상태 System이 체력을 보고 한다.
    public static class DamageSystem
    {
        /// <summary>
        /// 진행 중이면 피해 요청을 반영하고, 진행 중이 아니면 들어온 요청을 버린다. 체력이 이미 0인 대상은 피해를 받지 않는다.
        /// </summary>
        /// <param name="progress">한 판의 진행 상태. 처치 점수를 더한다</param>
        /// <param name="players">플레이어 저장소</param>
        /// <param name="enemies">적 저장소</param>
        /// <param name="bosses">보스 저장소</param>
        /// <param name="damageRequests">피해 요청 큐</param>
        public static void Tick(ref GameProgressComponent progress, PlayerStorage players, EnemyStorage enemies, BossStorage bosses, Queue<DamageRequest> damageRequests)
        {
            if (progress.phase != GamePhase.Playing)
            {
                if (damageRequests.Count > 0)
                {
                    Debug.LogWarning($"DamageSystem >> 진행 단계가 {progress.phase}일 때 들어온 피해 요청 {damageRequests.Count}개를 버렸다.");
                    damageRequests.Clear();
                }
                return;
            }

            int missingCount = 0;
            while (damageRequests.Count > 0)
            {
                DamageRequest request = damageRequests.Dequeue();
                bool hasTarget;
                switch (request.target.kind)
                {
                    case EntityKind.Player:
                        hasTarget = DamagePlayer(players, in request.target.identifier, request.amount);
                        break;
                    case EntityKind.Enemy:
                        hasTarget = DamageEnemy(ref progress, enemies, in request.target.identifier, request.amount);
                        break;
                    case EntityKind.Boss:
                        hasTarget = DamageBoss(ref progress, bosses, in request.target.identifier, request.amount);
                        break;
                    default:
                        hasTarget = false;
                        break;
                }

                if (!hasTarget)
                {
                    missingCount++;
                }
            }

            if (missingCount > 0)
            {
                Debug.LogWarning($"DamageSystem >> 대상이 없는 피해 요청 {missingCount}개를 버렸다.");
            }
        }

        private static bool DamagePlayer(PlayerStorage players, in EntityIdentifier target, float amount)
        {
            if (target.index < 0 || target.index >= players.entities.Length)
            {
                return false;
            }

            ref PlayerEntity player = ref players.entities[target.index];
            if (!player.isAlive || player.generation != target.generation)
            {
                return false;
            }

            ApplyDamage(ref player.health, amount);
            return true;
        }

        private static bool DamageEnemy(ref GameProgressComponent progress, EnemyStorage enemies, in EntityIdentifier target, float amount)
        {
            if (target.index < 0 || target.index >= enemies.entities.Length)
            {
                return false;
            }

            ref EnemyEntity enemy = ref enemies.entities[target.index];
            if (!enemy.isAlive || enemy.generation != target.generation)
            {
                return false;
            }

            if (ApplyDamage(ref enemy.health, amount))
            {
                progress.score += enemy.stats.score * enemy.level;
            }
            return true;
        }

        private static bool DamageBoss(ref GameProgressComponent progress, BossStorage bosses, in EntityIdentifier target, float amount)
        {
            if (target.index < 0 || target.index >= bosses.entities.Length)
            {
                return false;
            }

            ref BossEntity boss = ref bosses.entities[target.index];
            if (!boss.isAlive || boss.generation != target.generation)
            {
                return false;
            }

            if (ApplyDamage(ref boss.health, amount))
            {
                progress.score += boss.stats.score * boss.level;
            }
            return true;
        }

        // 체력을 깎는다. 이번 피해로 체력이 0이 되었으면 true를 돌려준다.
        private static bool ApplyDamage(ref float health, float amount)
        {
            if (health <= 0f)
            {
                return false;
            }

            health = Mathf.Max(0f, health - amount);
            return health <= 0f;
        }
    }
}
