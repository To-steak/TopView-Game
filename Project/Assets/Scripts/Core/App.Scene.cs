using TopView.Commands;
using TopView.Components;
using TopView.Definitions;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TopView.Core
{
    // 씬과 앱: World 불러오기와 로딩, 재시작, Menu로 돌아가기, 종료
    public partial class App
    {
        // 씬 이름과 최소 로딩 시간
        [SerializeField] private MenuSettingsDefinition _menuSettingsDefinition;

        // Awake에서 Menu Settings Definition에서 복사한 씬 흐름 값
        private MenuRuleComponent _rule;
        // 흐름 값을 복사했는지. Menu Settings Definition이 없으면 false이고, 이때는 씬을 바꾸는 명령을 버린다
        private bool _hasRule;
        // 시작 버튼으로 로딩을 요청했다. 이때부터 로딩 화면을 보인다
        private bool _isLoadingRequested;
        // 로딩을 요청한 프레임(Time.frameCount). 이 프레임에 로딩 화면이 그려진 뒤 불러오기를 시작한다
        private int _loadingRequestedFrame;
        // 불러오는 중인 씬. 불러오기 전에는 null이다
        private AsyncOperation _loading;
        // 로딩을 요청한 시각(Time.unscaledTime, 초). 최소 시간은 로딩 화면이 보인 때부터 잰다
        private float _loadingStartTime;

        // AsyncOperation.progress는 씬 활성화를 막아 두면 불러오기가 끝났을 때 0.9에서 멈춘다
        private const float LOADED_PROGRESS = 0.9f;

        private void ReadMenuRule()
        {
            if (_menuSettingsDefinition == null)
            {
                Debug.LogError("App >> Menu Settings Definition이 연결되어 있지 않아 World 불러오기와 Menu로 돌아가기를 하지 않는다.");
                return;
            }

            _rule = _menuSettingsDefinition.rule;
            _hasRule = true;
            if (_rule.minimumLoadingTicks < 0)
            {
                Debug.LogWarning($"App >> Menu Settings Definition의 Minimum Loading Ticks가 {_rule.minimumLoadingTicks}라 최소 시간 없이 불러오기가 끝나면 들어가기 버튼을 누를 수 있게 한다.");
            }
        }

        // 씬 명령을 모두 소비한다. 씬을 바꾸면 남은 명령은 이전 씬의 것이라 버린다.
        private void HandleSceneCommands()
        {
            for (int commandIndex = 0; commandIndex < _sceneCommands.Count; commandIndex++)
            {
                if (HandleSceneCommand(_sceneCommands[commandIndex].type))
                {
                    break;
                }
            }
            _sceneCommands.Clear();
        }

        // 씬을 바꾸거나 앱을 끝냈으면 true를 돌려준다
        private bool HandleSceneCommand(SceneCommandType type)
        {
            // 로딩을 요청하면 메뉴가 숨겨지므로, 그 뒤에 남은 명령은 정상적으로 무시한다. 로딩 화면의 들어가기만 받는다
            if (_isLoadingRequested && type != SceneCommandType.EnterWorld)
            {
                return false;
            }

            switch (type)
            {
                case SceneCommandType.LoadWorld:
                    RequestLoadingWorld();
                    return false;
                case SceneCommandType.EnterWorld:
                    EnterWorld();
                    return false;
                case SceneCommandType.Restart:
                    Restart();
                    return true;
                case SceneCommandType.ReturnToMenu:
                    return ReturnToMenu();
                case SceneCommandType.Exit:
                    SaveVolume();
                    Quit();
                    return true;
                default:
                    Debug.LogWarning($"App >> 처리하지 않는 씬 명령 {type}을 버렸다.");
                    return false;
            }
        }

        // 로딩 화면을 보이도록 요청만 한다. 불러오기는 다음 프레임의 BeginLoadingWorld가 시작한다.
        private void RequestLoadingWorld()
        {
            if (!_hasRule)
            {
                Debug.LogError("App >> Menu Settings Definition이 연결되어 있지 않아 World 불러오기 명령을 버렸다.");
                return;
            }

            if (!Application.CanStreamedLevelBeLoaded(_rule.worldSceneName))
            {
                Debug.LogError($"App >> 씬 {_rule.worldSceneName}이 Build Settings에 없어 불러올 수 없다.");
                return;
            }

            // 설정을 닫지 않고 시작해도 바꾼 볼륨이 남도록 먼저 저장한다
            SaveVolume();
            _isSettingsOpen = false;
            _isLoadingRequested = true;
            _loadingRequestedFrame = Time.frameCount;
            _loadingStartTime = Time.unscaledTime;
        }

        private void BeginLoadingWorld()
        {
            _loading = SceneManager.LoadSceneAsync(_rule.worldSceneName);
            // 들어가기 버튼을 누를 때까지 씬 활성화를 막아 둔다
            _loading.allowSceneActivation = false;
        }

        // 불러오기와 최소 시간이 끝났으면 막아 둔 씬 활성화를 풀어 World로 넘어간다.
        private void EnterWorld()
        {
            if (_loading == null || ReadLoadingRatio() < 1f)
            {
                Debug.LogWarning("App >> 아직 불러오기가 끝나지 않아 들어가기 명령을 버렸다.");
                return;
            }

            // 버튼을 여러 번 눌러도 이미 true라 결과가 같다
            _loading.allowSceneActivation = true;
        }

        // 실제 불러온 비율과 최소 시간 중 덜 지난 쪽을 0~1로 돌려준다. 1이면 씬을 바꿔도 된다. 불러오기 전이면 0이다
        private float ReadLoadingRatio()
        {
            if (_loading == null)
            {
                return 0f;
            }

            float loadedRatio = Mathf.Clamp01(_loading.progress / LOADED_PROGRESS);
            if (_rule.minimumLoadingTicks <= 0)
            {
                return loadedRatio;
            }

            float minimumSeconds = _rule.minimumLoadingTicks * Time.fixedDeltaTime;
            float timeRatio = Mathf.Clamp01((Time.unscaledTime - _loadingStartTime) / minimumSeconds);
            return Mathf.Min(loadedRatio, timeRatio);
        }

        // 지금 씬을 다시 불러 새 판을 준비한다. 씬이 바뀌기까지 남은 프레임에 판이 돌지 않도록 등록을 먼저 푼다
        private void Restart()
        {
            ReleaseScene();
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        // Menu 씬을 불러온다. 씬을 불러오지 못하면 지금 씬을 그대로 두고 false를 돌려준다
        private bool ReturnToMenu()
        {
            if (!_hasRule)
            {
                Debug.LogError("App >> Menu Settings Definition이 연결되어 있지 않아 Menu로 돌아가지 않았다.");
                return false;
            }

            string menuSceneName = _rule.menuSceneName;
            if (!Application.CanStreamedLevelBeLoaded(menuSceneName))
            {
                Debug.LogError($"App >> Menu Settings Definition의 Menu Scene Name {menuSceneName}이 Build Settings에 없어 Menu로 돌아가지 않았다.");
                return false;
            }

            ReleaseScene();
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
