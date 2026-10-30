using System.Collections.Generic;
using TopView.Commands;
using TopView.Components;
using TopView.Requests;
using UnityEngine;

namespace TopView.Systems
{
    // 명령 버퍼의 명령을 소비해 게임 상태에 반영한다.
    public static class CommandSystem
    {
        /// <summary>
        /// 판 흐름 명령 중 Start를 소비해 진행 단계를 Playing으로 바꾸고 플레이어 생성 요청을 넣는다. Restart와 Exit는 Runner가 소비하므로 남겨 둔다.
        /// </summary>
        /// <param name="gameFlowCommands">판 흐름 명령 목록</param>
        /// <param name="progress">한 판의 진행 상태</param>
        /// <param name="playerSpawnTemplate">판을 준비할 때 Core가 채워 둔 플레이어 생성 요청</param>
        /// <param name="playerSpawnRequests">플레이어 생성 요청 큐</param>
        public static void Tick(List<GameFlowCommand> gameFlowCommands, ref GameProgressComponent progress, in PlayerSpawnRequest playerSpawnTemplate, Queue<PlayerSpawnRequest> playerSpawnRequests)
        {
            for (int commandIndex = gameFlowCommands.Count - 1; commandIndex >= 0; commandIndex--)
            {
                if (gameFlowCommands[commandIndex].type != GameFlowCommandType.Start)
                {
                    continue;
                }

                gameFlowCommands.RemoveAt(commandIndex);

                if (progress.phase != GamePhase.Ready)
                {
                    Debug.LogWarning($"CommandSystem >> 진행 단계가 {progress.phase}일 때 들어온 Start 명령을 버렸다.");
                    continue;
                }

                progress.phase = GamePhase.Playing;
                playerSpawnRequests.Enqueue(playerSpawnTemplate);
            }
        }

        /// <summary>
        /// 플레이어 입력 중 누름 입력을 지운다. 이동, 달리기, 조준처럼 계속 유지되는 입력은 남긴다. 틱 마지막에 부른다.
        /// </summary>
        /// <param name="playerCommand">플레이어 입력</param>
        public static void ClearPlayerPresses(ref PlayerCommand playerCommand)
        {
            playerCommand.isJumpPressed = false;
            playerCommand.isDodgePressed = false;
            playerCommand.isAttackPressed = false;
            playerCommand.isReloadPressed = false;
            playerCommand.swapWeaponIndex = -1;
        }
    }
}
