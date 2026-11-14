using System.Collections.Generic;
using TopView.Components;
using TopView.Entities;
using TopView.Requests;
using UnityEngine;

namespace TopView.Systems
{
    // 발동 요청을 받은 적 스포너가 생성 주기마다 적 생성 요청을 넣는다. 최대 마릿수를 다 만들면 멈추고, 다시 발동하지 않는다. 무한 스포너는 멈추지 않는다.
    public static class EnemySpawnerSystem
    {
        // 발동 요청의 연결 번호로 찾는 스포너
        private const ComponentMask LINKED_SPAWNER_MASK = ComponentMask.SpawnerLink | ComponentMask.EnemySpawnerState;
        // 적을 내보내는 스포너
        private const ComponentMask SPAWNER_MASK = ComponentMask.Position | ComponentMask.Rotation | ComponentMask.EnemySpawner | ComponentMask.EnemySpawnerRule | ComponentMask.EnemySpawnerState;

        /// <summary>
        /// 발동 요청을 처리한 뒤, 작동 중인 스포너의 타이머를 1틱 줄이고 0이 되면 적 생성 요청을 넣는다. 그 적 종류의 빈 칸이 없으면 만든 것으로 세지 않고 다음 틱에 다시 본다.
        /// </summary>
        /// <param name="enemyLevel">생성할 적의 레벨</param>
        /// <param name="storage">엔티티 저장소. 발동은 SpawnerLink, EnemySpawnerState를 가진 칸에서 연결 번호로 찾고, 생성은 Position, Rotation, EnemySpawner, EnemySpawnerRule, EnemySpawnerState를 가진 칸만 처리한다. enemySpawnerState를 고치고, 적 종류별 빈 칸 수를 읽는다</param>
        /// <param name="enemySpawnTemplates">판을 준비할 때 Core가 종류별로 Definition 값을 채워 둔 적 생성 요청</param>
        /// <param name="enemySpawnerActivationRequests">적 스포너 발동 요청 큐</param>
        /// <param name="enemySpawnRequests">적 생성 요청 큐</param>
        public static void Tick(int enemyLevel, EntityStorage storage, EnemySpawnRequest[] enemySpawnTemplates, Queue<EnemySpawnerActivationRequest> enemySpawnerActivationRequests, Queue<EnemySpawnRequest> enemySpawnRequests)
        {
            Entity[] entities = storage.entities;
            int missingCount = 0;
            while (enemySpawnerActivationRequests.Count > 0)
            {
                EnemySpawnerActivationRequest request = enemySpawnerActivationRequests.Dequeue();
                if (!TryActivateLinked(entities, request.spawnerLinkIndex))
                {
                    missingCount++;
                }
            }

            if (missingCount > 0)
            {
                Debug.LogWarning($"EnemySpawnerSystem >> 연결 번호에 맞는 적 스포너가 없어 발동 요청 {missingCount}개를 버렸다.");
            }

            int waitingCount = 0;
            for (int index = 0; index < entities.Length; index++)
            {
                ref Entity entity = ref entities[index];
                if ((entity.mask & SPAWNER_MASK) != SPAWNER_MASK)
                {
                    continue;
                }

                if (!TryTickTimer(ref entity.enemySpawnerState))
                {
                    continue;
                }

                // 빈 칸이 없는데 요청을 넣으면 EnemySpawnSystem이 버리고, 스포너는 만든 것으로 세어 그만큼 적을 덜 만든다. 빈 칸이 생길 때까지 다음 틱에 다시 본다
                int enemyKindIndex = entity.enemySpawner.enemyKindIndex;
                if (storage.freeEnemyIndicesByKind[enemyKindIndex].Count <= CountQueued(enemySpawnRequests, enemyKindIndex))
                {
                    entity.enemySpawnerState.remainingTicks = 1;
                    waitingCount++;
                    continue;
                }

                CountSpawned(ref entity.enemySpawnerState, in entity.enemySpawnerRule);
                EnemySpawnRequest enemySpawnRequest = enemySpawnTemplates[enemyKindIndex];
                enemySpawnRequest.position = entity.position.current;
                enemySpawnRequest.rotation = entity.rotation.current;
                enemySpawnRequest.level = enemyLevel;
                enemySpawnRequests.Enqueue(enemySpawnRequest);
            }

            if (waitingCount > 0)
            {
                Debug.LogWarning($"EnemySpawnerSystem >> 적 종류의 빈 칸이 없어 적 스포너 {waitingCount}개가 생성을 다음 틱으로 미뤘다.");
            }
        }

        // 이번 틱에 이미 넣은 같은 종류의 적 생성 요청 수를 센다.
        private static int CountQueued(Queue<EnemySpawnRequest> enemySpawnRequests, int enemyKindIndex)
        {
            int count = 0;
            foreach (EnemySpawnRequest request in enemySpawnRequests)
            {
                if (request.enemyKindIndex == enemyKindIndex)
                {
                    count++;
                }
            }
            return count;
        }

        // 연결 번호가 같은 스포너를 찾아 발동한다. 찾지 못하면 false다.
        private static bool TryActivateLinked(Entity[] entities, int spawnerLinkIndex)
        {
            for (int index = 0; index < entities.Length; index++)
            {
                ref Entity entity = ref entities[index];
                if ((entity.mask & LINKED_SPAWNER_MASK) == LINKED_SPAWNER_MASK && entity.spawnerLink.linkIndex == spawnerLinkIndex)
                {
                    Activate(ref entity.enemySpawnerState);
                    return true;
                }
            }

            return false;
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

        // 작동 중인 스포너의 타이머를 1틱 줄이고, 0이 되면 true를 돌려준다.
        private static bool TryTickTimer(ref EnemySpawnerStateComponent state)
        {
            if (!state.isActive)
            {
                return false;
            }

            state.remainingTicks--;
            return state.remainingTicks <= 0;
        }

        // 한 마리를 내보낸 것으로 세고 타이머를 다시 맞춘다. 최대 마릿수를 다 만들면 멈춘다.
        private static void CountSpawned(ref EnemySpawnerStateComponent state, in EnemySpawnerRuleComponent rule)
        {
            state.spawnedCount++;
            state.remainingTicks = rule.spawnIntervalTicks;
            state.isActive = rule.isInfinite || state.spawnedCount < rule.maxSpawnCount;
        }
    }
}
