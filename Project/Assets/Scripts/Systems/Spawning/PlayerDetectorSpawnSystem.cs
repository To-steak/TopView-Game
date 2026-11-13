using System.Collections.Generic;
using TopView.Components;
using TopView.Entities;
using TopView.Requests;
using TopView.Utilities;
using UnityEngine;

namespace TopView.Systems
{
    // 플레이어 감지기 생성 요청을 받아 저장소 플레이어 감지기 구간의 빈 칸을 채운다. 틱 마지막에 부른다.
    public static class PlayerDetectorSpawnSystem
    {
        /// <summary>
        /// 플레이어 감지기 생성 요청을 모두 처리한다. 빈 칸이 없으면 요청을 버린다.
        /// </summary>
        /// <param name="storage">엔티티 저장소. 플레이어 감지기 구간의 칸을 채운다</param>
        /// <param name="playerDetectorSpawnRequests">플레이어 감지기 생성 요청 큐</param>
        public static void Tick(EntityStorage storage, Queue<PlayerDetectorSpawnRequest> playerDetectorSpawnRequests)
        {
            int droppedCount = 0;
            while (playerDetectorSpawnRequests.Count > 0)
            {
                PlayerDetectorSpawnRequest request = playerDetectorSpawnRequests.Dequeue();
                if (!SpawnUtility.TryTakeFreeIndex(storage.freePlayerDetectorIndices, out int index))
                {
                    droppedCount++;
                    continue;
                }

                Spawn(ref storage.entities[index], in request);
            }

            if (droppedCount > 0)
            {
                Debug.LogWarning($"PlayerDetectorSpawnSystem >> 플레이어 감지기 구간에 빈 칸이 없어 생성 요청 {droppedCount}개를 버렸다.");
            }
        }

        // 칸 전체를 새 값으로 바꿔 이전 값이 남지 않게 한다. 연결 번호 배열만 다시 쓴다.
        private static void Spawn(ref Entity entity, in PlayerDetectorSpawnRequest request)
        {
            int generation = entity.generation + 1;
            int[] spawnerLinkIndices = SpawnUtility.ReuseArray(entity.playerDetectorLinks.spawnerLinkIndices, request.links.spawnerLinkIndices);

            entity = new Entity
            {
                mask = EntityArchetype.PLAYER_DETECTOR,
                generation = generation,
                position = new PositionComponent { previous = request.position, current = request.position },
                playerDetectorRange = request.range,
                playerDetectorState = new PlayerDetectorStateComponent(),
                playerDetectorLinks = new PlayerDetectorLinksComponent { spawnerLinkIndices = spawnerLinkIndices }
            };
        }
    }
}
