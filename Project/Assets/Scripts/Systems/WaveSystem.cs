using System.Collections.Generic;
using TopView.Components;
using TopView.Requests;
using UnityEngine;

namespace TopView.Systems
{
    // 적 종류별 스폰 타이머와 웨이브 시작을 처리해 생성 요청을 넣는다.
    public static class WaveSystem
    {
        /// <summary>
        /// 스폰 중인 적 종류의 타이머를 1틱 줄여 적 생성 요청을 넣고, 시작 시각이 된 웨이브를 시작한다.
        /// </summary>
        /// <param name="rule">게임 규칙 값</param>
        /// <param name="waves">웨이브 목록</param>
        /// <param name="enemySpawnPointsByKind">적 종류별 생기는 지점 목록</param>
        /// <param name="enemySpawnTemplates">판을 준비할 때 Core가 종류별로 Definition 값을 채워 둔 적 생성 요청</param>
        /// <param name="bossSpawnTemplate">판을 준비할 때 Core가 Definition 값과 위치를 채워 둔 보스 생성 요청</param>
        /// <param name="progress">한 판의 진행 상태</param>
        /// <param name="random">World의 난수 생성기</param>
        /// <param name="enemySpawnRequests">적 생성 요청 큐</param>
        /// <param name="bossSpawnRequests">보스 생성 요청 큐</param>
        public static void Tick(in GameRuleComponent rule, WaveComponent[] waves, SpawnPointComponent[][] enemySpawnPointsByKind, EnemySpawnRequest[] enemySpawnTemplates, in BossSpawnRequest bossSpawnTemplate, ref GameProgressComponent progress, System.Random random, Queue<EnemySpawnRequest> enemySpawnRequests, Queue<BossSpawnRequest> bossSpawnRequests)
        {
            int droppedRequestCount = 0;

            for (int enemyKindIndex = 0; enemyKindIndex < progress.isEnemyKindSpawning.Length; enemyKindIndex++)
            {
                if (!progress.isEnemyKindSpawning[enemyKindIndex])
                {
                    continue;
                }

                progress.enemyKindSpawnRemainingTicks[enemyKindIndex]--;
                if (progress.enemyKindSpawnRemainingTicks[enemyKindIndex] <= 0)
                {
                    if (!TryRequestEnemy(in rule, enemyKindIndex, enemySpawnPointsByKind, enemySpawnTemplates, random, enemySpawnRequests))
                    {
                        droppedRequestCount++;
                    }
                    progress.enemyKindSpawnRemainingTicks[enemyKindIndex] = rule.spawnIntervalTicks;
                }
            }

            int elapsedTicks = rule.timeLimitTicks - progress.remainingTicks;
            for (int waveIndex = 0; waveIndex < waves.Length; waveIndex++)
            {
                ref readonly WaveComponent wave = ref waves[waveIndex];
                if (progress.isWaveStarted[waveIndex] || elapsedTicks < wave.startTicks)
                {
                    continue;
                }

                progress.isWaveStarted[waveIndex] = true;

                if (wave.isBoss)
                {
                    BossSpawnRequest bossRequest = bossSpawnTemplate;
                    bossRequest.level = rule.bossLevel;
                    bossSpawnRequests.Enqueue(bossRequest);
                    continue;
                }

                if (wave.enemyKindIndex < 0 || wave.enemyKindIndex >= progress.isEnemyKindSpawning.Length)
                {
                    Debug.LogWarning($"WaveSystem >> 웨이브 {waveIndex}의 적 종류 번호 {wave.enemyKindIndex}가 범위 밖이라 건너뛰었다.");
                    continue;
                }

                progress.isEnemyKindSpawning[wave.enemyKindIndex] = true;
                if (!TryRequestEnemy(in rule, wave.enemyKindIndex, enemySpawnPointsByKind, enemySpawnTemplates, random, enemySpawnRequests))
                {
                    droppedRequestCount++;
                }
                progress.enemyKindSpawnRemainingTicks[wave.enemyKindIndex] = rule.spawnIntervalTicks;
            }

            if (droppedRequestCount > 0)
            {
                Debug.LogWarning($"WaveSystem >> 스폰 지점이 없어 적 생성 요청 {droppedRequestCount}개를 버렸다.");
            }
        }

        // 그 적 종류의 스폰 지점 하나를 무작위로 골라 적 생성 요청을 넣는다. 스폰 지점이 없으면 false를 돌려준다.
        private static bool TryRequestEnemy(in GameRuleComponent rule, int enemyKindIndex, SpawnPointComponent[][] enemySpawnPointsByKind, EnemySpawnRequest[] enemySpawnTemplates, System.Random random, Queue<EnemySpawnRequest> enemySpawnRequests)
        {
            SpawnPointComponent[] spawnPoints = enemySpawnPointsByKind[enemyKindIndex];
            if (spawnPoints.Length == 0)
            {
                return false;
            }

            ref readonly SpawnPointComponent spawnPoint = ref spawnPoints[random.Next(0, spawnPoints.Length)];
            EnemySpawnRequest request = enemySpawnTemplates[enemyKindIndex];
            request.enemyKindIndex = enemyKindIndex;
            request.position = spawnPoint.position;
            request.rotation = spawnPoint.rotation;
            request.level = rule.enemyLevel;
            enemySpawnRequests.Enqueue(request);
            return true;
        }
    }
}
