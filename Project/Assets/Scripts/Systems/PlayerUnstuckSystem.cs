using System.Collections.Generic;
using TopView.Commands;
using TopView.Components;
using TopView.Entities;
using TopView.Utilities;
using UnityEngine;
using UnityEngine.AI;

namespace TopView.Systems
{
    // 판 흐름 명령 중 탈출을 처리한다. 땅 아래로 빠진 플레이어를 가까운 NavMesh 위로 옮긴다.
    public static class PlayerUnstuckSystem
    {
        /// <summary>
        /// Unstuck 명령을 소비해 살아 있는 플레이어를 가까운 NavMesh 위로 옮긴다. 다른 명령은 남겨 둔다. 진행 중이 아니면 명령만 버린다.
        /// 일시정지 화면에서 들어온 명령을 처리하므로 PauseSystem 뒤에서 일시정지와 상관없이 부른다.
        /// </summary>
        /// <param name="players">플레이어 저장소</param>
        /// <param name="progress">한 판의 진행 상태</param>
        /// <param name="rule">한 판의 규칙 값</param>
        /// <param name="gameFlowCommands">판 흐름 명령 목록</param>
        public static void Tick(PlayerStorage players, in GameProgressComponent progress, in GameRuleComponent rule, List<GameFlowCommand> gameFlowCommands)
        {
            for (int commandIndex = 0; commandIndex < gameFlowCommands.Count; commandIndex++)
            {
                if (gameFlowCommands[commandIndex].type != GameFlowCommandType.Unstuck)
                {
                    continue;
                }

                gameFlowCommands.RemoveAt(commandIndex);
                commandIndex--;
                // 판이 끝난 뒤에 늦게 도착한 명령은 정상적으로 무시한다
                if (progress.phase == GamePhase.Playing)
                {
                    MoveAlivePlayers(players, in rule);
                }
            }
        }

        private static void MoveAlivePlayers(PlayerStorage players, in GameRuleComponent rule)
        {
            bool hasAlivePlayer = false;
            for (int index = 0; index < players.entities.Length; index++)
            {
                ref PlayerEntity entity = ref players.entities[index];
                if (!entity.isAlive)
                {
                    continue;
                }

                hasAlivePlayer = true;
                if (!TryFindDestination(in entity.position, in entity.movement, entity.rotation.current, in rule, out Vector3 destination))
                {
                    Debug.LogWarning($"PlayerUnstuckSystem >> 플레이어 위치 {entity.position.current}에서 {rule.unstuckSampleDistance}유닛 안에 NavMesh가 없어 옮기지 않았다. Game Settings Definition의 Unstuck 값을 확인한다.");
                    continue;
                }

                Place(ref entity.position, ref entity.motion, destination);
            }

            if (!hasAlivePlayer)
            {
                Debug.LogWarning("PlayerUnstuckSystem >> 살아 있는 플레이어가 없어 탈출 명령을 버렸다.");
            }
        }

        // 지금 XZ에서 가장 가까운 NavMesh 점을 찾고, 캡슐 바닥이 그 점 위에 skinWidth만큼 떠 있는 위치를 돌려준다.
        // 기준 높이보다 아래로 빠졌으면 기준 높이에서 찾아, 빠진 깊이와 상관없이 그 자리 위로 올린다
        private static bool TryFindDestination(in PositionComponent position, in PlayerMovementComponent movement, Quaternion rotation, in GameRuleComponent rule, out Vector3 destination)
        {
            Vector3 samplePoint = position.current;
            samplePoint.y = Mathf.Max(samplePoint.y, rule.unstuckSampleMinimumHeight);
            if (!NavMesh.SamplePosition(samplePoint, out NavMeshHit hit, rule.unstuckSampleDistance, NavMesh.AllAreas))
            {
                destination = position.current;
                return false;
            }

            PlayerMovementUtility.GetCapsulePoints(in movement, rotation, Vector3.zero, out Vector3 top, out Vector3 bottom);
            float capsuleLowestHeight = bottom.y - movement.capsuleRadius;
            destination = hit.position + Vector3.up * (movement.skinWidth - capsuleLowestHeight);
            return true;
        }

        // 보간이 옛 위치에서 끌려오지 않도록 이전 위치도 함께 옮기고, 떨어지던 속도를 없앤다
        private static void Place(ref PositionComponent position, ref PlayerMotionComponent motion, Vector3 destination)
        {
            position.previous = destination;
            position.current = destination;
            motion.verticalVelocity = 0f;
        }
    }
}
