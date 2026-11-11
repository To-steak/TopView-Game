using System.Collections.Generic;
using TopView.Commands;
using TopView.Components;
using UnityEngine;

namespace TopView.Systems
{
    // 판 흐름 명령 중 일시정지와 재개, 강화 화면 열기와 닫기를 처리한다.
    public static class PauseSystem
    {
        /// <summary>
        /// Pause, Resume, OpenUpgrade, CloseUpgrade 명령을 소비해 일시정지 상태와 강화 화면 상태를 바꾼다. 다른 명령은 남겨 둔다. 진행 중이 아니면 일시정지하지 않고 명령만 버린다. 틱 맨 앞에서 부른다.
        /// </summary>
        /// <param name="progress">한 판의 진행 상태</param>
        /// <param name="gameFlowCommands">판 흐름 명령 목록</param>
        public static void Tick(ref GameProgressComponent progress, List<GameFlowCommand> gameFlowCommands)
        {
            for (int commandIndex = 0; commandIndex < gameFlowCommands.Count; commandIndex++)
            {
                GameFlowCommandType type = gameFlowCommands[commandIndex].type;
                if (type != GameFlowCommandType.Pause && type != GameFlowCommandType.Resume && type != GameFlowCommandType.OpenUpgrade && type != GameFlowCommandType.CloseUpgrade)
                {
                    continue;
                }

                gameFlowCommands.RemoveAt(commandIndex);
                commandIndex--;
                Apply(ref progress, type);
            }
        }

        // 판이 끝난 뒤의 입력은 정상적으로 무시한다. 강화 화면은 ESC 일시정지와 같은 일시정지에 화면만 다르게 보인다.
        // 이미 일시정지 중에 강화 화면을 열거나, 열려 있지 않은 강화 화면을 닫는 명령은 늦게 도착한 입력이라 정상적으로 무시한다
        private static void Apply(ref GameProgressComponent progress, GameFlowCommandType type)
        {
            if (progress.phase != GamePhase.Playing)
            {
                progress.isPaused = false;
                progress.isUpgradeOpen = false;
                return;
            }

            switch (type)
            {
                case GameFlowCommandType.Pause:
                    if (progress.isUpgradeOpen)
                    {
                        Debug.LogWarning("PauseSystem >> 강화 화면이 열려 있어 Pause 명령을 버렸다.");
                        return;
                    }
                    progress.isPaused = true;
                    break;
                case GameFlowCommandType.Resume:
                    progress.isPaused = false;
                    progress.isUpgradeOpen = false;
                    break;
                case GameFlowCommandType.OpenUpgrade:
                    if (!progress.isPaused)
                    {
                        progress.isPaused = true;
                        progress.isUpgradeOpen = true;
                    }
                    break;
                case GameFlowCommandType.CloseUpgrade:
                    if (progress.isUpgradeOpen)
                    {
                        progress.isPaused = false;
                        progress.isUpgradeOpen = false;
                    }
                    break;
            }
        }
    }
}
