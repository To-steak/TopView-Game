using System.Collections.Generic;
using TopView.Components;
using TopView.Entities;
using TopView.Requests;
using UnityEngine;

namespace TopView.Systems
{
    // 플레이어가 감지 범위에 처음 들어오면 연결된 적 스포너의 발동을 요청한다. 감지기마다 한 번만 감지한다.
    public static class PlayerDetectorSystem
    {
        private const ComponentMask DETECTOR_MASK = ComponentMask.Position | ComponentMask.PlayerDetectorRange | ComponentMask.PlayerDetectorState | ComponentMask.PlayerDetectorLinks;

        /// <summary>
        /// 아직 감지하지 않은 감지기 중 플레이어가 범위에 든 감지기를 감지 완료로 바꾸고, 연결된 적 스포너마다 발동을 요청한다.
        /// </summary>
        /// <param name="storage">엔티티 저장소. Position, PlayerDetectorRange, PlayerDetectorState, PlayerDetectorLinks를 가진 칸만 처리하고, playerDetectorState를 고친다</param>
        /// <param name="playerLocation">이번 틱의 플레이어 위치</param>
        /// <param name="enemySpawnerActivationRequests">적 스포너 발동 요청 큐</param>
        public static void Tick(EntityStorage storage, in PlayerLocationComponent playerLocation, Queue<EnemySpawnerActivationRequest> enemySpawnerActivationRequests)
        {
            if (!playerLocation.hasPlayer)
            {
                return;
            }

            Entity[] entities = storage.entities;
            for (int index = 0; index < entities.Length; index++)
            {
                ref Entity entity = ref entities[index];
                if ((entity.mask & DETECTOR_MASK) != DETECTOR_MASK)
                {
                    continue;
                }

                if (entity.playerDetectorState.hasTriggered || !IsPlayerInRange(in entity.position, in entity.playerDetectorRange, in playerLocation))
                {
                    continue;
                }

                entity.playerDetectorState.hasTriggered = true;
                int[] spawnerLinkIndices = entity.playerDetectorLinks.spawnerLinkIndices;
                for (int linkIndex = 0; linkIndex < spawnerLinkIndices.Length; linkIndex++)
                {
                    enemySpawnerActivationRequests.Enqueue(new EnemySpawnerActivationRequest { spawnerLinkIndex = spawnerLinkIndices[linkIndex] });
                }
            }
        }

        // XZ 평면에서 플레이어 캡슐 가장자리가 감지 원에 닿았는지 본다.
        private static bool IsPlayerInRange(in PositionComponent position, in PlayerDetectorRangeComponent range, in PlayerLocationComponent playerLocation)
        {
            Vector3 offset = playerLocation.position - position.current;
            offset.y = 0f;

            float reach = range.detectionRadius + playerLocation.radius;
            return offset.sqrMagnitude <= reach * reach;
        }
    }
}
