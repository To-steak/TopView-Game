using System.Collections.Generic;
using TopView.Components;
using TopView.Requests;
using UnityEngine;

namespace TopView.Systems
{
    // 플레이어가 감지 범위에 처음 들어오면 연결된 적 스포너의 발동을 요청한다. 감지기마다 한 번만 감지한다.
    public static class PlayerDetectorSystem
    {
        /// <summary>
        /// 아직 감지하지 않은 감지기 중 플레이어가 범위에 든 감지기를 감지 완료로 바꾸고, 연결된 적 스포너마다 발동을 요청한다.
        /// </summary>
        /// <param name="detectors">플레이어 감지기 목록</param>
        /// <param name="states">플레이어 감지기 상태 목록. 번호가 detectors와 같다</param>
        /// <param name="playerLocation">이번 틱의 플레이어 위치</param>
        /// <param name="enemySpawnerActivationRequests">적 스포너 발동 요청 큐</param>
        public static void Tick(PlayerDetectorComponent[] detectors, PlayerDetectorStateComponent[] states, in PlayerLocationComponent playerLocation, Queue<EnemySpawnerActivationRequest> enemySpawnerActivationRequests)
        {
            if (!playerLocation.hasPlayer)
            {
                return;
            }

            for (int detectorIndex = 0; detectorIndex < detectors.Length; detectorIndex++)
            {
                ref readonly PlayerDetectorComponent detector = ref detectors[detectorIndex];
                ref PlayerDetectorStateComponent state = ref states[detectorIndex];
                if (state.hasTriggered || !IsPlayerInRange(in detector, in playerLocation))
                {
                    continue;
                }

                state.hasTriggered = true;
                for (int linkIndex = 0; linkIndex < detector.spawnerIndices.Length; linkIndex++)
                {
                    enemySpawnerActivationRequests.Enqueue(new EnemySpawnerActivationRequest { spawnerIndex = detector.spawnerIndices[linkIndex] });
                }
            }
        }

        // XZ 평면에서 플레이어 캡슐 가장자리가 감지 원에 닿았는지 본다.
        private static bool IsPlayerInRange(in PlayerDetectorComponent detector, in PlayerLocationComponent playerLocation)
        {
            Vector3 offset = playerLocation.position - detector.position;
            offset.y = 0f;
            float range = detector.range.detectionRadius + playerLocation.radius;
            return offset.sqrMagnitude <= range * range;
        }
    }
}
