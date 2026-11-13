using System.Collections.Generic;
using TopView.Components;
using TopView.Entities;
using TopView.Requests;
using TopView.Utilities;
using UnityEngine;

namespace TopView.Systems
{
    // 적 스포너 생성 요청을 받아 저장소 적 스포너 구간의 빈 칸을 채운다. 틱 마지막에 부른다.
    public static class EnemySpawnerSpawnSystem
    {
        /// <summary>
        /// 적 스포너 생성 요청을 모두 처리한다. 빈 칸이 없으면 요청을 버린다.
        /// </summary>
        /// <param name="storage">엔티티 저장소. 적 스포너 구간의 칸을 채운다</param>
        /// <param name="enemySpawnerSpawnRequests">적 스포너 생성 요청 큐</param>
        public static void Tick(EntityStorage storage, Queue<EnemySpawnerSpawnRequest> enemySpawnerSpawnRequests)
        {
            int droppedCount = 0;
            while (enemySpawnerSpawnRequests.Count > 0)
            {
                EnemySpawnerSpawnRequest request = enemySpawnerSpawnRequests.Dequeue();
                if (!SpawnUtility.TryTakeFreeIndex(storage.freeEnemySpawnerIndices, out int index))
                {
                    droppedCount++;
                    continue;
                }

                Spawn(ref storage.entities[index], in request);
            }

            if (droppedCount > 0)
            {
                Debug.LogWarning($"EnemySpawnerSpawnSystem >> 적 스포너 구간에 빈 칸이 없어 생성 요청 {droppedCount}개를 버렸다.");
            }
        }

        // 칸 전체를 새 값으로 바꿔 이전 값이 남지 않게 한다.
        private static void Spawn(ref Entity entity, in EnemySpawnerSpawnRequest request)
        {
            int generation = entity.generation + 1;

            entity = new Entity
            {
                mask = EntityArchetype.ENEMY_SPAWNER,
                generation = generation,
                position = new PositionComponent { previous = request.position, current = request.position },
                rotation = new RotationComponent { previous = request.rotation, current = request.rotation },
                enemySpawner = request.enemySpawner,
                enemySpawnerRule = request.rule,
                enemySpawnerState = new EnemySpawnerStateComponent(),
                spawnerLink = request.link
            };
        }
    }
}
