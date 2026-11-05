using System.Collections.Generic;
using TopView.Components;
using TopView.Requests;

namespace TopView.Systems
{
    // 발동 요청을 받은 적 스포너가 생성 주기마다 적 생성 요청을 넣는다. 최대 마릿수를 다 만들면 멈추고, 다시 발동하지 않는다.
    public static class EnemySpawnerSystem
    {
        /// <summary>
        /// 발동 요청을 처리한 뒤, 작동 중인 스포너의 타이머를 1틱 줄이고 0이 되면 적 생성 요청을 넣는다.
        /// </summary>
        /// <param name="rule">게임 규칙 값</param>
        /// <param name="spawners">적 스포너 목록</param>
        /// <param name="states">적 스포너 상태 목록. 번호가 spawners와 같다</param>
        /// <param name="enemySpawnTemplates">판을 준비할 때 Core가 종류별로 Definition 값을 채워 둔 적 생성 요청</param>
        /// <param name="enemySpawnerActivationRequests">적 스포너 발동 요청 큐</param>
        /// <param name="enemySpawnRequests">적 생성 요청 큐</param>
        public static void Tick(in GameRuleComponent rule, EnemySpawnerComponent[] spawners, EnemySpawnerStateComponent[] states, EnemySpawnRequest[] enemySpawnTemplates, Queue<EnemySpawnerActivationRequest> enemySpawnerActivationRequests, Queue<EnemySpawnRequest> enemySpawnRequests)
        {
            while (enemySpawnerActivationRequests.Count > 0)
            {
                EnemySpawnerActivationRequest request = enemySpawnerActivationRequests.Dequeue();
                Activate(ref states[request.spawnerIndex]);
            }

            for (int spawnerIndex = 0; spawnerIndex < spawners.Length; spawnerIndex++)
            {
                ref readonly EnemySpawnerComponent spawner = ref spawners[spawnerIndex];
                ref EnemySpawnerStateComponent state = ref states[spawnerIndex];
                if (!state.isActive)
                {
                    continue;
                }

                state.remainingTicks--;
                if (state.remainingTicks > 0)
                {
                    continue;
                }

                EnemySpawnRequest enemySpawnRequest = enemySpawnTemplates[spawner.enemyKindIndex];
                enemySpawnRequest.position = spawner.position;
                enemySpawnRequest.rotation = spawner.rotation;
                enemySpawnRequest.level = rule.enemyLevel;
                enemySpawnRequests.Enqueue(enemySpawnRequest);

                state.spawnedCount++;
                state.remainingTicks = spawner.rule.spawnIntervalTicks;
                state.isActive = state.spawnedCount < spawner.rule.maxSpawnCount;
            }
        }

        // 아직 발동한 적 없는 스포너만 발동한다. 발동한 틱에 바로 첫 적을 내보내도록 타이머를 1로 둔다.
        private static void Activate(ref EnemySpawnerStateComponent state)
        {
            if (state.isActive || state.spawnedCount > 0)
            {
                return;
            }

            state.isActive = true;
            state.remainingTicks = 1;
        }
    }
}
