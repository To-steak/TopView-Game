using System.Collections.Generic;
using TopView.Components;
using TopView.Entities;
using TopView.Records;
using TopView.Requests;
using UnityEngine;

namespace TopView.Systems
{
    // 보스 피해 요청을 체력에 반영하고, 체력이 0이 되면 사망 상태로 바꾸며 점수를 더한다.
    public static class BossDamageSystem
    {
        /// <summary>
        /// 진행 중이면 피해 요청을 반영하고, 진행 중이 아니면 들어온 요청을 버린다. 이미 사망한 보스는 피해를 받지 않는다.
        /// </summary>
        /// <param name="progress">한 판의 진행 상태. 점수를 더한다</param>
        /// <param name="bosses">보스 저장소</param>
        /// <param name="bossDamageRequests">보스 피해 요청 큐</param>
        /// <param name="bossStateRecords">보스 상태 변경 기록</param>
        public static void Tick(ref GameProgressComponent progress, BossStorage bosses, Queue<BossDamageRequest> bossDamageRequests, List<BossStateChangedRecord> bossStateRecords)
        {
            if (progress.phase != GamePhase.Playing)
            {
                if (bossDamageRequests.Count > 0)
                {
                    Debug.LogWarning($"BossDamageSystem >> 진행 단계가 {progress.phase}일 때 들어온 피해 요청 {bossDamageRequests.Count}개를 버렸다.");
                    bossDamageRequests.Clear();
                }
                return;
            }

            int missingCount = 0;
            while (bossDamageRequests.Count > 0)
            {
                BossDamageRequest request = bossDamageRequests.Dequeue();
                if (!IsAlive(bosses, in request.target))
                {
                    missingCount++;
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

            if (missingCount > 0)
            {
                Debug.LogWarning($"BossDamageSystem >> 대상 보스가 없는 피해 요청 {missingCount}개를 버렸다.");
            }
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
