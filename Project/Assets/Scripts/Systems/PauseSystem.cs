using System.Collections.Generic;
using TopView.Commands;
using TopView.Components;

namespace TopView.Systems
{
    // 판 흐름 명령 중 일시정지와 재개를 처리한다.
    public static class PauseSystem
    {
        /// <summary>
        /// Pause와 Resume 명령을 소비해 일시정지 상태를 바꾼다. 다른 명령은 남겨 둔다. 진행 중이 아니면 일시정지하지 않고 명령만 버린다. 틱 맨 앞에서 부른다.
        /// </summary>
        /// <param name="progress">한 판의 진행 상태</param>
        /// <param name="gameFlowCommands">판 흐름 명령 목록</param>
        public static void Tick(ref GameProgressComponent progress, List<GameFlowCommand> gameFlowCommands)
        {
            for (int commandIndex = 0; commandIndex < gameFlowCommands.Count; commandIndex++)
            {
                GameFlowCommandType type = gameFlowCommands[commandIndex].type;
                if (type != GameFlowCommandType.Pause && type != GameFlowCommandType.Resume)
                {
                    continue;
                }

                gameFlowCommands.RemoveAt(commandIndex);
                commandIndex--;
                Apply(ref progress, type);
            }
        }

        // 판이 끝난 뒤의 일시정지 입력은 정상적으로 무시한다
        private static void Apply(ref GameProgressComponent progress, GameFlowCommandType type)
        {
            if (progress.phase != GamePhase.Playing)
            {
                progress.isPaused = false;
                return;
            }

            progress.isPaused = type == GameFlowCommandType.Pause;
        }
    }
}
