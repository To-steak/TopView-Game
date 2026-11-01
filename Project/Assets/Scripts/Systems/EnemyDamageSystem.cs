using System.Collections.Generic;
using TopView.Components;
using TopView.Entities;
using TopView.Records;
using TopView.Requests;
using UnityEngine;

namespace TopView.Systems
{
    // 적 피해 요청을 체력에 반영하고, 체력이 0이 되면 사망 상태로 바꾸며 점수를 더한다.
    public static class EnemyDamageSystem
    {
        /// <summary>
        /// 진행 중이면 피해 요청을 반영하고, 진행 중이 아니면 들어온 요청을 버린다. 이미 사망한 적은 피해를 받지 않는다.
        /// </summary>
        /// <param name="progress">한 판의 진행 상태. 점수를 더한다</param>
        /// <param name="enemies">적 저장소</param>
        /// <param name="enemyDamageRequests">적 피해 요청 큐</param>
        /// <param name="enemyStateRecords">적 상태 변경 기록</param>
        public static void Tick(ref GameProgressComponent progress, EnemyStorage enemies, Queue<EnemyDamageRequest> enemyDamageRequests, List<EnemyStateChangedRecord> enemyStateRecords)
        {
            if (progress.phase != GamePhase.Playing)
            {
                if (enemyDamageRequests.Count > 0)
                {
                    Debug.LogWarning($"EnemyDamageSystem >> 진행 단계가 {progress.phase}일 때 들어온 피해 요청 {enemyDamageRequests.Count}개를 버렸다.");
                    enemyDamageRequests.Clear();
                }
                return;
            }

            int missingCount = 0;
            while (enemyDamageRequests.Count > 0)
            {
                EnemyDamageRequest request = enemyDamageRequests.Dequeue();
                if (!IsAlive(enemies, in request.target))
                {
                    missingCount++;
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

            if (missingCount > 0)
            {
                Debug.LogWarning($"EnemyDamageSystem >> 대상 적이 없는 피해 요청 {missingCount}개를 버렸다.");
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
    }
}
