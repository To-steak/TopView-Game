using System.Collections.Generic;
using TopView.Components;
using TopView.Entities;
using TopView.Records;
using TopView.Requests;
using UnityEngine;

namespace TopView.Systems
{
    // 적·보스 피해 요청을 체력에 반영하고, 체력이 0이 되면 사망 상태로 바꾸며 점수를 더한다.
    public static class EnemyDamageSystem
    {
        /// <summary>
        /// 진행 중이면 피해 요청을 반영하고, 진행 중이 아니면 들어온 요청을 버린다. 이미 사망한 적·보스는 피해를 받지 않는다.
        /// </summary>
        /// <param name="progress">한 판의 진행 상태. 점수를 더한다</param>
        /// <param name="enemies">적 저장소</param>
        /// <param name="bosses">보스 저장소</param>
        /// <param name="enemyDamageRequests">적 피해 요청 큐</param>
        /// <param name="bossDamageRequests">보스 피해 요청 큐</param>
        /// <param name="enemyStateRecords">적 상태 변경 기록</param>
        /// <param name="bossStateRecords">보스 상태 변경 기록</param>
        public static void Tick(ref GameProgressComponent progress, EnemyStorage enemies, BossStorage bosses, Queue<EnemyDamageRequest> enemyDamageRequests, Queue<BossDamageRequest> bossDamageRequests, List<EnemyStateChangedRecord> enemyStateRecords, List<BossStateChangedRecord> bossStateRecords)
        {
            if (progress.phase != GamePhase.Playing)
            {
                DiscardRequests(progress.phase, enemyDamageRequests, bossDamageRequests);
                return;
            }

            int missingEnemyCount = 0;
            while (enemyDamageRequests.Count > 0)
            {
                EnemyDamageRequest request = enemyDamageRequests.Dequeue();
                if (!IsAlive(enemies, in request.target))
                {
                    missingEnemyCount++;
                    continue;
                }

                ref EnemyEntity enemy = ref enemies.entities[request.target.index];
                if (enemy.state == EnemyState.Die)
                {
                    continue;
                }

                enemy.health = Mathf.Max(0f, enemy.health - request.amount);
                if (enemy.health <= 0f)
                {
                    progress.score += enemy.stats.score * enemy.level;
                    EnemyBehaviorSystem.ChangeState(ref enemy, in request.target, EnemyState.Die, enemyStateRecords);
                }
            }

            int missingBossCount = 0;
            while (bossDamageRequests.Count > 0)
            {
                BossDamageRequest request = bossDamageRequests.Dequeue();
                if (!IsAlive(bosses, in request.target))
                {
                    missingBossCount++;
                    continue;
                }

                ref BossEntity boss = ref bosses.entities[request.target.index];
                if (boss.state == BossState.Die)
                {
                    continue;
                }

                boss.health = Mathf.Max(0f, boss.health - request.amount);
                if (boss.health <= 0f)
                {
                    progress.score += boss.stats.score * boss.level;
                    BossBehaviorSystem.ChangeState(ref boss, in request.target, BossState.Die, bossStateRecords);
                }
            }

            if (missingEnemyCount > 0)
            {
                Debug.LogWarning($"EnemyDamageSystem >> 대상 적이 없는 피해 요청 {missingEnemyCount}개를 버렸다.");
            }
            if (missingBossCount > 0)
            {
                Debug.LogWarning($"EnemyDamageSystem >> 대상 보스가 없는 피해 요청 {missingBossCount}개를 버렸다.");
            }
        }

        private static void DiscardRequests(GamePhase phase, Queue<EnemyDamageRequest> enemyDamageRequests, Queue<BossDamageRequest> bossDamageRequests)
        {
            int requestCount = enemyDamageRequests.Count + bossDamageRequests.Count;
            if (requestCount > 0)
            {
                Debug.LogWarning($"EnemyDamageSystem >> 진행 단계가 {phase}일 때 들어온 피해 요청 {requestCount}개를 버렸다.");
                enemyDamageRequests.Clear();
                bossDamageRequests.Clear();
            }
        }

        private static bool IsAlive(EnemyStorage enemies, in EntityIdentifier identifier)
        {
            if (identifier.index < 0 || identifier.index >= enemies.entities.Length)
            {
                return false;
            }

            ref readonly EnemyEntity enemy = ref enemies.entities[identifier.index];
            return enemy.isAlive && enemy.generation == identifier.generation;
        }

        private static bool IsAlive(BossStorage bosses, in EntityIdentifier identifier)
        {
            if (identifier.index < 0 || identifier.index >= bosses.entities.Length)
            {
                return false;
            }

            ref readonly BossEntity boss = ref bosses.entities[identifier.index];
            return boss.isAlive && boss.generation == identifier.generation;
        }
    }
}
