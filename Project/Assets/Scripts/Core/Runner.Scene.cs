using System.Collections.Generic;
using TopView.Commands;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TopView.Core
{
    // 씬과 앱: 재시작, 종료, Menu로 돌아가기
    public partial class Runner
    {
        // 씬 명령을 모두 소비한다. 씬과 앱의 전환이라 Tick이 아닌 여기서 처리한다. 씬을 바꾸면 남은 명령은 이전 판의 것이라 버린다.
        private void HandleSceneCommands(List<SceneCommand> sceneCommands)
        {
            for (int commandIndex = sceneCommands.Count - 1; commandIndex >= 0; commandIndex--)
            {
                SceneCommandType type = sceneCommands[commandIndex].type;
                switch (type)
                {
                    case SceneCommandType.Restart:
                        sceneCommands.Clear();
                        ClearRound();
                        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
                        return;
                    case SceneCommandType.Exit:
                        sceneCommands.Clear();
                        Quit();
                        return;
                    case SceneCommandType.ReturnToMenu:
                        if (ReturnToMenu())
                        {
                            sceneCommands.Clear();
                            return;
                        }
                        break;
                    default:
                        Debug.LogWarning($"Runner >> 처리하지 않는 씬 명령 {type}을 버렸다.");
                        break;
                }
            }
            sceneCommands.Clear();
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
