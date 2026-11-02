using System.Collections.Generic;
using TopView.Components;
using TopView.Entities;
using TopView.Requests;
using UnityEngine;

namespace TopView.Systems
{
    // 피해 요청을 플레이어·적·보스의 체력에 반영하고, 적·보스를 처치하면 점수와 골드를 더한다. 사망 상태로 바꾸는 일은 각 상태 System이 체력을 보고 한다.
    public static class DamageSystem
    {
        // 처치 보상을 주는 엔티티가 가지는 컴포넌트
        private const ComponentMask KILL_REWARD_QUERY = ComponentMask.Combat | ComponentMask.EnemyStats;

        /// <summary>
        /// 진행 중이면 피해 요청을 반영하고, 진행 중이 아니면 들어온 요청을 버린다. 체력이 이미 0인 대상은 피해를 받지 않는다.
        /// </summary>
        /// <param name="progress">한 판의 진행 상태. 처치 점수와 골드를 더한다</param>
        /// <param name="players">플레이어 저장소</param>
        /// <param name="masks">공유 저장소의 칸별 컴포넌트 표시</param>
        /// <param name="generations">공유 저장소의 칸이 채워진 횟수</param>
        /// <param name="healths">공유 저장소의 체력</param>
        /// <param name="combats">공유 저장소의 레벨과 공격력</param>
        /// <param name="enemyStats">공유 저장소의 점수·골드 값</param>
        /// <param name="damageRequests">피해 요청 큐</param>
        public static void Tick(ref GameProgressComponent progress, PlayerStorage players, ComponentMask[] masks, int[] generations, HealthComponent[] healths, CombatComponent[] combats, EnemyStatsComponent[] enemyStats, Queue<DamageRequest> damageRequests)
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
                    case EntityKind.Shared:
                        hasTarget = DamageEntity(ref progress, masks, generations, healths, combats, enemyStats, in request.target.identifier, request.amount);
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

        // 체력을 가진 엔티티에 피해를 준다. 처치 보상 값까지 가졌으면 처치할 때 점수와 골드를 더한다.
        private static bool DamageEntity(ref GameProgressComponent progress, ComponentMask[] masks, int[] generations, HealthComponent[] healths, CombatComponent[] combats, EnemyStatsComponent[] enemyStats, in EntityIdentifier target, float amount)
        {
            if (target.index < 0 || target.index >= masks.Length)
            {
                return false;
            }

            ComponentMask mask = masks[target.index];
            if ((mask & ComponentMask.Health) == 0 || generations[target.index] != target.generation)
            {
                return false;
            }

            if (ApplyDamage(ref healths[target.index], amount) && (mask & KILL_REWARD_QUERY) == KILL_REWARD_QUERY)
            {
                AddKillReward(ref progress, in enemyStats[target.index], combats[target.index].level);
            }
            return true;
        }

        // 체력을 깎는다. 이번 피해로 체력이 0이 되었으면 true를 돌려준다.
        private static bool ApplyDamage(ref HealthComponent health, float amount)
        {
            if (health.current <= 0f)
            {
                return false;
            }

            health.current = Mathf.Max(0f, health.current - amount);
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
