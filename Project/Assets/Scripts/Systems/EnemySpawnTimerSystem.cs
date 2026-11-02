using System.Collections.Generic;
using TopView.Components;
using TopView.Requests;

namespace TopView.Systems
{
    // 판 시작부터 스폰 간격마다 모든 스폰 지점에 적 생성 요청을 넣는다.
    public static class EnemySpawnTimerSystem
    {
        /// <summary>
        /// 타이머를 1틱 줄이고, 0이 되면 스폰 지점마다 적 생성 요청을 넣은 뒤 타이머를 스폰 간격으로 되돌린다.
        /// </summary>
        /// <param name="rule">게임 규칙 값</param>
        /// <param name="spawnPoints">적이 생기는 지점 목록</param>
        /// <param name="enemySpawnTemplates">판을 준비할 때 Core가 종류별로 Definition 값을 채워 둔 적 생성 요청</param>
        /// <param name="progress">한 판의 진행 상태</param>
        /// <param name="enemySpawnRequests">적 생성 요청 큐</param>
        public static void Tick(in GameRuleComponent rule, SpawnPointComponent[] spawnPoints, EnemySpawnRequest[] enemySpawnTemplates, ref GameProgressComponent progress, Queue<EnemySpawnRequest> enemySpawnRequests)
        {
            progress.enemySpawnRemainingTicks--;
            if (progress.enemySpawnRemainingTicks > 0)
            {
                return;
            }

            progress.enemySpawnRemainingTicks = rule.spawnIntervalTicks;
            for (int spawnPointIndex = 0; spawnPointIndex < spawnPoints.Length; spawnPointIndex++)
            {
                ref readonly SpawnPointComponent spawnPoint = ref spawnPoints[spawnPointIndex];
                EnemySpawnRequest request = enemySpawnTemplates[spawnPoint.enemyKindIndex];
                request.position = spawnPoint.position;
                request.rotation = spawnPoint.rotation;
                request.level = rule.enemyLevel;
                enemySpawnRequests.Enqueue(request);
            }
        }
    }
}
