using System.Collections.Generic;
using TopView.Commands;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TopView.Core
{
    // 씬과 앱: 재시작과 종료
    public partial class Runner
    {
        // Restart와 Exit 명령을 소비한다. 씬과 앱의 전환이라 Tick이 아닌 여기서 처리한다.
        private void HandleSceneCommands(List<GameFlowCommand> gameFlowCommands)
        {
            for (int commandIndex = gameFlowCommands.Count - 1; commandIndex >= 0; commandIndex--)
            {
                GameFlowCommandType type = gameFlowCommands[commandIndex].type;
                if (type == GameFlowCommandType.Restart)
                {
                    gameFlowCommands.RemoveAt(commandIndex);
                    ClearRound();
                    SceneManager.LoadScene(SceneManager.GetActiveScene().name);
                    return;
                }

                if (type == GameFlowCommandType.Exit)
                {
                    gameFlowCommands.RemoveAt(commandIndex);
                    Quit();
                    return;
                }
            }
        }

        private static void Quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
