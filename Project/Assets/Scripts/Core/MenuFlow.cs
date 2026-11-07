using System.Collections.Generic;
using TopView.Commands;
using TopView.Components;
using TopView.Definitions;
using TopView.Persistence;
using TopView.Presentation.ViewContexts;
using TopView.Presentation.ViewSystems;
using TopView.UserInterface.Presenters;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TopView.Core
{
    // Menu 씬에 하나 배치해 메뉴의 흐름을 맡는다. 메뉴 Presenter를 연결하고, 메뉴 명령을 소비하고, World 씬을 비동기로 불러온다.
    public class MenuFlow : MonoBehaviour
    {
        [SerializeField] private MenuPresenter _menuPresenter;
        [SerializeField] private SettingsPresenter _settingsPresenter;
        [SerializeField] private LoadingPresenter _loadingPresenter;
        // 볼륨을 조절할 Audio Mixer와 볼륨 시작값
        [SerializeField] private AudioSettingsDefinition _audioSettingsDefinition;
        // 불러올 씬 이름과 최소 로딩 시간
        [SerializeField] private MenuSettingsDefinition _menuSettingsDefinition;

        private readonly List<MenuCommand> _commands = new List<MenuCommand>();
        // Start에서 Menu Settings Definition에서 복사한 흐름 값
        private MenuRuleComponent _rule;
        // 흐름 값을 복사했는지. Menu Settings Definition이 없으면 false이고, 이때는 World를 불러오지 않는다
        private bool _hasRule;
        private AudioVolumeComponent _volume;
        // 볼륨 반영 실행 상태. Audio Settings Definition이 없으면 null이고, 이때는 볼륨을 반영하지도 저장하지도 않는다
        private AudioVolumeViewContext _audioVolumeViewContext;
        // 슬라이더 값이 0~1 밖이라는 경고를 이미 남겼는지. 슬라이더를 움직이는 동안 매 프레임 쌓이지 않게 한 번만 남긴다
        private bool _hasReportedVolumeOutOfRange;
        private bool _isSettingsOpen;
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

        // 씬의 다른 오브젝트가 Awake를 마친 뒤에 연결한다. Audio Mixer는 Awake에서 값을 무시할 수 있어 볼륨도 여기서 반영한다
        private void Start()
        {
            if (_menuSettingsDefinition == null)
            {
                Debug.LogError("MenuFlow >> Menu Settings Definition이 연결되어 있지 않아 시작 버튼을 눌러도 World를 불러오지 않는다.");
            }
            else
            {
                _rule = _menuSettingsDefinition.rule;
                _hasRule = true;
                if (_rule.minimumLoadingTicks < 0)
                {
                    Debug.LogWarning($"MenuFlow >> Menu Settings Definition의 Minimum Loading Ticks가 {_rule.minimumLoadingTicks}라 최소 시간 없이 불러오기가 끝나면 들어가기 버튼을 보인다.");
                }
            }

            if (_audioSettingsDefinition == null)
            {
                // 저장된 볼륨을 1로 덮어쓰지 않도록 저장도 하지 않는다
                Debug.LogWarning("MenuFlow >> Audio Settings Definition이 연결되어 있지 않아 설정 화면의 볼륨을 1로 보이고, Audio Mixer 반영과 저장을 하지 않는다.");
                _volume = new AudioVolumeComponent { musicVolume = 1f, soundEffectVolume = 1f };
            }
            else
            {
                _volume = AudioVolumeStore.Load(in _audioSettingsDefinition.settings);
                _audioVolumeViewContext = new AudioVolumeViewContext { mixer = _audioSettingsDefinition.mixer };
                AudioVolumeViewSystem.Present(in _volume, _audioVolumeViewContext);
            }

            if (_menuPresenter == null)
            {
                Debug.LogError("MenuFlow >> Menu Presenter가 연결되어 있지 않다.");
            }
            else
            {
                _menuPresenter.Initialize(_commands);
            }

            if (_settingsPresenter == null)
            {
                Debug.LogError("MenuFlow >> Settings Presenter가 연결되어 있지 않다.");
            }
            else
            {
                _settingsPresenter.Initialize(_commands);
            }

            if (_loadingPresenter == null)
            {
                Debug.LogError("MenuFlow >> Loading Presenter가 연결되어 있지 않다.");
            }
            else
            {
                _loadingPresenter.Initialize(_commands);
            }
        }

        private void Update()
        {
            for (int commandIndex = 0; commandIndex < _commands.Count; commandIndex++)
            {
                HandleCommand(_commands[commandIndex]);
            }
            _commands.Clear();

            // LoadSceneAsync는 부른 프레임에 메인 스레드를 멈출 수 있어, 로딩 화면이 한 번 그려진 다음 프레임에 부른다
            if (_isLoadingRequested && _loading == null && Time.frameCount > _loadingRequestedFrame)
            {
                BeginLoadingWorld();
            }
        }

        private void LateUpdate()
        {
            bool isLoading = _isLoadingRequested;
            if (_menuPresenter != null)
            {
                _menuPresenter.Present(!isLoading);
            }
            if (_settingsPresenter != null)
            {
                _settingsPresenter.Present(_isSettingsOpen && !isLoading, in _volume);
            }
            if (_loadingPresenter != null)
            {
                float ratio = isLoading ? ReadLoadingRatio() : 0f;
                _loadingPresenter.Present(isLoading, ratio, ratio >= 1f);
            }
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

        private void HandleCommand(in MenuCommand command)
        {
            // 로딩을 요청하면 메뉴가 숨겨지므로, 그 뒤에 남은 메뉴 명령은 정상적으로 무시한다. 로딩 화면의 들어가기만 받는다
            if (_isLoadingRequested && command.type != MenuCommandType.EnterWorld)
            {
                return;
            }

            switch (command.type)
            {
                case MenuCommandType.Start:
                    RequestLoadingWorld();
                    break;
                case MenuCommandType.EnterWorld:
                    EnterWorld();
                    break;
                case MenuCommandType.OpenSettings:
                    _isSettingsOpen = true;
                    break;
                case MenuCommandType.CloseSettings:
                    _isSettingsOpen = false;
                    SaveVolume();
                    break;
                case MenuCommandType.SetMusicVolume:
                    _volume.musicVolume = ReadVolume(in command);
                    PresentVolume();
                    break;
                case MenuCommandType.SetSoundEffectVolume:
                    _volume.soundEffectVolume = ReadVolume(in command);
                    PresentVolume();
                    break;
                case MenuCommandType.Exit:
                    SaveVolume();
                    Quit();
                    break;
                default:
                    Debug.LogWarning($"MenuFlow >> 처리하지 않는 메뉴 명령 {command.type}을 버렸다.");
                    break;
            }
        }

        // 로딩 화면을 보이도록 요청만 한다. 불러오기는 다음 프레임의 BeginLoadingWorld가 시작한다.
        private void RequestLoadingWorld()
        {
            if (!_hasRule)
            {
                Debug.LogError("MenuFlow >> Menu Settings Definition이 연결되어 있지 않아 시작 명령을 버렸다.");
                return;
            }

            if (!Application.CanStreamedLevelBeLoaded(_rule.worldSceneName))
            {
                Debug.LogError($"MenuFlow >> 씬 {_rule.worldSceneName}이 Build Settings에 없어 불러올 수 없다.");
                return;
            }

            // World의 Runner가 저장된 볼륨을 다시 읽으므로, 설정을 닫지 않고 시작해도 바꾼 볼륨이 남도록 먼저 저장한다
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
                Debug.LogWarning("MenuFlow >> 아직 불러오기가 끝나지 않아 들어가기 명령을 버렸다.");
                return;
            }

            // 버튼을 여러 번 눌러도 이미 true라 결과가 같다
            _loading.allowSceneActivation = true;
        }

        private float ReadVolume(in MenuCommand command)
        {
            if (command.value < 0f || command.value > 1f)
            {
                if (!_hasReportedVolumeOutOfRange)
                {
                    Debug.LogWarning($"MenuFlow >> {command.type} 볼륨 {command.value}이 0~1 밖이라 범위 안으로 맞춘다. 슬라이더의 Min/Max Value를 확인한다. 이 경고는 한 번만 남긴다.");
                    _hasReportedVolumeOutOfRange = true;
                }
                return Mathf.Clamp01(command.value);
            }

            return command.value;
        }

        // Audio Settings Definition이 없으면 Start에서 경고했으므로 로그 없이 건너뛴다
        private void PresentVolume()
        {
            if (_audioVolumeViewContext != null)
            {
                AudioVolumeViewSystem.Present(in _volume, _audioVolumeViewContext);
            }
        }

        // Audio Settings Definition이 없으면 저장된 볼륨을 시작값 1로 덮어쓰지 않도록 저장하지 않는다. Start에서 경고했다
        private void SaveVolume()
        {
            if (_audioVolumeViewContext != null)
            {
                AudioVolumeStore.Save(in _volume);
            }
        }

        // Runner.Quit과 같은 코드다. Runner를 나눌 때 함께 정리한다
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
