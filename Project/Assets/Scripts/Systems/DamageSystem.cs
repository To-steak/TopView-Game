using System.Collections.Generic;
using TopView.Components;
using TopView.Entities;
using TopView.Requests;
using UnityEngine;

namespace TopView.Systems
{
    // 피해 요청을 방향별로 체력에 반영하고, 적을 처치하면 점수와 골드를 더한다. 사망 상태로 바꾸는 일은 각 상태 System이 체력을 보고 한다.
    public static class DamageSystem
    {
        /// <summary>
        /// 진행 중이면 피해 요청을 반영하고, 진행 중이 아니면 들어온 요청을 버린다. 체력이 이미 0인 대상은 피해를 받지 않는다.
        /// </summary>
        /// <param name="progress">한 판의 진행 상태. 처치 점수와 골드를 더한다</param>
        /// <param name="masks">칸별 컴포넌트 표시</param>
        /// <param name="generations">칸이 채워진 횟수</param>
        /// <param name="healths">체력</param>
        /// <param name="combats">레벨과 공격력</param>
        /// <param name="enemyStats">점수·골드 값</param>
        /// <param name="playerDamageRequests">플레이어 피해 요청 큐</param>
        /// <param name="enemyDamageRequests">적 피해 요청 큐</param>
        public static void Tick(ref GameProgressComponent progress, ComponentMask[] masks, int[] generations, HealthComponent[] healths, CombatComponent[] combats, EnemyStatsComponent[] enemyStats, Queue<DamageRequest> playerDamageRequests, Queue<DamageRequest> enemyDamageRequests)
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
                if (!HasTarget(masks, generations, in request.target))
                {
                    missingCount++;
                    continue;
                }

                Apply(ref healths[request.target.index], in request);
            }

            while (enemyDamageRequests.Count > 0)
            {
                DamageRequest request = enemyDamageRequests.Dequeue();
                if (!HasTarget(masks, generations, in request.target))
                {
                    missingCount++;
                    continue;
                }

                int index = request.target.index;
                if (Apply(ref healths[index], in request))
                {
                    AddKillReward(ref progress, in enemyStats[index], combats[index].level);
                }
            }

            if (missingCount > 0)
            {
                Debug.LogWarning($"DamageSystem >> 대상이 없는 피해 요청 {missingCount}개를 버렸다.");
            }
        }

        private static bool HasTarget(ComponentMask[] masks, int[] generations, in EntityIdentifier target)
        {
            return target.index >= 0 && target.index < masks.Length
                && (masks[target.index] & ComponentMask.Health) != 0
                && generations[target.index] == target.generation;
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

        // 처치한 대상의 레벨에 맞는 점수와 골드를 더한다.
        private static void AddKillReward(ref GameProgressComponent progress, in EnemyStatsComponent stats, int level)
        {
            progress.score += stats.baseScore + level * stats.scoreIncrement;
            progress.gold += stats.baseGold + level * stats.goldIncrement;
        }
    }
}
