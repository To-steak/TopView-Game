using System.Collections.Generic;
using TopView.Components;
using TopView.Entities;
using TopView.Requests;
using UnityEngine;

namespace TopView.Systems
{
    // 피해 요청을 방향별로 체력에 반영하고, 적을 처치하면 점수를 더한다. 사망 상태로 바꾸는 일은 각 상태 System이 체력을 보고 한다.
    public static class DamageSystem
    {
        /// <summary>
        /// 진행 중이면 피해 요청을 반영하고, 진행 중이 아니면 들어온 요청을 버린다. 체력이 이미 0인 대상은 피해를 받지 않는다.
        /// </summary>
        /// <param name="progress">한 판의 진행 상태. 처치 점수를 더한다</param>
        /// <param name="players">플레이어 저장소</param>
        /// <param name="enemies">적 저장소</param>
        /// <param name="playerDamageRequests">플레이어 피해 요청 큐</param>
        /// <param name="enemyDamageRequests">적 피해 요청 큐</param>
        public static void Tick(ref GameProgressComponent progress, PlayerStorage players, EnemyStorage enemies, Queue<DamageRequest> playerDamageRequests, Queue<DamageRequest> enemyDamageRequests)
        {
            if (progress.phase != GamePhase.Playing)
            {
                int droppedCount = playerDamageRequests.Count + enemyDamageRequests.Count;
                if (droppedCount > 0)
                {
                    Debug.LogWarning($"DamageSystem >> 진행 단계가 {progress.phase}일 때 들어온 피해 요청 {droppedCount}개를 버렸다.");
                    playerDamageRequests.Clear();
                    enemyDamageRequests.Clear();
                }
                return;
            }

            int missingCount = 0;

            while (playerDamageRequests.Count > 0)
            {
                DamageRequest request = playerDamageRequests.Dequeue();
                if (!HasPlayerTarget(players.entities, in request.target))
                {
                    missingCount++;
                    continue;
                }

                Apply(ref players.entities[request.target.index].health, in request);
            }

            while (enemyDamageRequests.Count > 0)
            {
                DamageRequest request = enemyDamageRequests.Dequeue();
                if (!HasEnemyTarget(enemies.entities, in request.target))
                {
                    missingCount++;
                    continue;
                }

                ref EnemyEntity enemy = ref enemies.entities[request.target.index];
                if (Apply(ref enemy.health, in request))
                {
                    AddKillScore(ref progress, in enemy.stats, enemy.combat.level);
                }
            }

            if (missingCount > 0)
            {
                Debug.LogWarning($"DamageSystem >> 대상이 없는 피해 요청 {missingCount}개를 버렸다.");
            }
        }

        private static bool HasPlayerTarget(PlayerEntity[] players, in EntityIdentifier target)
        {
            return target.index >= 0 && target.index < players.Length
                && players[target.index].isAlive
                && players[target.index].generation == target.generation;
        }

        private static bool HasEnemyTarget(EnemyEntity[] enemies, in EntityIdentifier target)
        {
            return target.index >= 0 && target.index < enemies.Length
                && enemies[target.index].isAlive
                && enemies[target.index].generation == target.generation;
        }

        // 체력을 깎는다. 이번 피해로 체력이 0이 되었으면 true를 돌려준다.
        private static bool Apply(ref HealthComponent health, in DamageRequest request)
        {
            if (health.current <= 0f)
            {
                return false;
            }

            health.current = Mathf.Max(0f, health.current - request.amount);
            return health.current <= 0f;
        }

        // 처치한 대상의 레벨에 맞는 점수를 더한다.
        private static void AddKillScore(ref GameProgressComponent progress, in EnemyStatsComponent stats, int level)
        {
            progress.score += stats.baseScore + level * stats.scoreIncrement;
        }
    }
}
