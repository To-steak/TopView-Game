using System.Collections.Generic;
using TopView.Components;
using TopView.Entities;
using TopView.Records;
using TopView.Requests;
using UnityEngine;

namespace TopView.Systems
{
    // 플레이어 피해 요청을 체력에 반영하고, 체력이 0이 되면 사망 상태로 바꾼다.
    public static class PlayerDamageSystem
    {
        /// <summary>
        /// 진행 중이면 피해 요청을 반영하고, 진행 중이 아니면 들어온 요청을 버린다. 이미 사망한 플레이어는 피해를 받지 않는다.
        /// </summary>
        /// <param name="phase">진행 단계</param>
        /// <param name="players">플레이어 저장소</param>
        /// <param name="damageRequests">플레이어 피해 요청 큐</param>
        /// <param name="stateRecords">상태 변경 기록</param>
        public static void Tick(GamePhase phase, PlayerStorage players, Queue<PlayerDamageRequest> damageRequests, List<PlayerStateChangedRecord> stateRecords)
        {
            if (phase != GamePhase.Playing)
            {
                if (damageRequests.Count > 0)
                {
                    Debug.LogWarning($"PlayerDamageSystem >> 진행 단계가 {phase}일 때 들어온 피해 요청 {damageRequests.Count}개를 버렸다.");
                    damageRequests.Clear();
                }
                return;
            }

            int missingTargetCount = 0;
            while (damageRequests.Count > 0)
            {
                PlayerDamageRequest request = damageRequests.Dequeue();
                if (!IsAlive(players, in request.target))
                {
                    missingTargetCount++;
                    continue;
                }

                ref PlayerEntity player = ref players.entities[request.target.index];
                if (player.state == PlayerState.Die)
                {
                    continue;
                }

                player.health -= request.amount;
                if (player.health <= 0f)
                {
                    player.health = 0f;
                    PlayerStateSystem.ChangeState(ref player, in request.target, PlayerState.Die, Vector3.zero, stateRecords);
                }
            }

            if (missingTargetCount > 0)
            {
                Debug.LogWarning($"PlayerDamageSystem >> 대상 플레이어가 없는 피해 요청 {missingTargetCount}개를 버렸다.");
            }
        }

        private static bool IsAlive(PlayerStorage players, in EntityIdentifier identifier)
        {
            if (identifier.index < 0 || identifier.index >= players.entities.Length)
            {
                return false;
            }

            ref readonly PlayerEntity player = ref players.entities[identifier.index];
            return player.isAlive && player.generation == identifier.generation;
        }
    }
}
