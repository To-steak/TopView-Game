using System.Collections.Generic;
using TopView.Commands;
using TopView.Components;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TopView.Core
{
    // 씬과 앱: 재시작, 종료, Menu로 돌아가기와 설정 화면 열기
    public partial class Runner
    {
        // Restart, Exit, ReturnToMenu, OpenSettings 명령을 소비한다. 씬과 앱의 전환이고 설정은 표시 설정이라 Tick이 아닌 여기서 처리한다.
        // Pause와 Resume은 PauseSystem이 소비하도록 남겨 두고, 처리하는 곳이 없는 명령은 경고하고 버린다.
        private void HandleSceneCommands(List<GameFlowCommand> gameFlowCommands, in GameProgressComponent progress)
        {
            for (int commandIndex = gameFlowCommands.Count - 1; commandIndex >= 0; commandIndex--)
            {
                GameFlowCommandType type = gameFlowCommands[commandIndex].type;
                switch (type)
                {
                    case GameFlowCommandType.Pause:
                    case GameFlowCommandType.Resume:
                        // PauseSystem이 다음 틱에 소비한다
                        break;
                    case GameFlowCommandType.Restart:
                        gameFlowCommands.RemoveAt(commandIndex);
                        ClearRound();
                        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
                        return;
                    case GameFlowCommandType.Exit:
                        gameFlowCommands.RemoveAt(commandIndex);
                        Quit();
                        return;
                    case GameFlowCommandType.ReturnToMenu:
                        gameFlowCommands.RemoveAt(commandIndex);
                        if (ReturnToMenu())
                        {
                            return;
                        }
                        break;
                    case GameFlowCommandType.OpenSettings:
                        gameFlowCommands.RemoveAt(commandIndex);
                        // 설정 버튼은 일시정지 화면에만 있어, 일시정지가 풀린 뒤에 남은 명령은 정상적으로 무시한다
                        if (progress.isPaused)
                        {
                            _isSettingsOpen = true;
                        }
                        break;
                    default:
                        // 처리하는 곳이 없는 명령이 목록에 쌓이지 않도록 경고하고 버린다
                        gameFlowCommands.RemoveAt(commandIndex);
                        Debug.LogWarning($"Runner >> 처리하는 곳이 없는 판 흐름 명령 {type}을 버렸다.");
                        break;
                }
            }
        }

        // 판을 끝내고 Menu 씬을 불러온다. 씬을 불러오지 못하면 판을 그대로 두고 false를 돌려준다
        private bool ReturnToMenu()
        {
            string menuSceneName = _world.flowRule.menuSceneName;
            if (!Application.CanStreamedLevelBeLoaded(menuSceneName))
            {
                Debug.LogError($"Runner >> Game Settings Definition의 Flow Rule > Menu Scene Name {menuSceneName}이 Build Settings에 없어 Menu로 돌아가지 않았다.");
                return false;
            }

            ClearRound();
            SceneManager.LoadScene(menuSceneName);
            return true;
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
