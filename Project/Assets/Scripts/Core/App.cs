using System.Collections.Generic;
using TopView.Commands;
using TopView.Components;
using TopView.Definitions;
using TopView.Presentation.ViewSystems;
using TopView.UserInterface.Presenters;
using UnityEngine;

namespace TopView.Core
{
    // 앱에 하나뿐인 DDOL 오브젝트. Menu와 World 씬이 함께 쓰고 앱의 수명을 따르는 일(볼륨과 설정 화면, 씬 전환·로딩·종료, 배경음악과 UI 소리 재생)을 맡는다.
    // Unity의 실행 함수는 여기서만 받고, 지금 씬의 World나 Menu를 정해진 순서로 부른다.
    // 요소별 상태와 처리는 App.<요소>.cs에 나눠 두고, 이 파일에는 씬 등록과 호출 순서만 둔다.
    public partial class App : MonoBehaviour
    {
        // 앱에 하나뿐인 App. 씬의 World와 Menu가 자기를 등록할 때 쓴다. 아직 없으면 null
        public static App Instance { get; private set; }

        // 지금 씬의 World. World 씬이 아니면 null
        private World _world;
        // 지금 씬의 Menu. Menu 씬이 아니면 null
        private Menu _menu;
        // 씬과 앱 전환 명령. 어느 씬의 Presenter가 넣었든 Update에서 소비한다
        private readonly List<SceneCommand> _sceneCommands = new List<SceneCommand>();
        // 설정 명령. 어느 씬의 Presenter가 넣었든 Update에서 소비한다
        private readonly List<SettingsCommand> _settingsCommands = new List<SettingsCommand>();
        // Presenter가 이번 프레임에 넣은 버튼 소리 요청. LateUpdate 끝에 비운다
        private readonly List<UserInterfaceSoundKind> _userInterfaceSoundRequests = new List<UserInterfaceSoundKind>();

        /// <summary>
        /// World 씬의 World를 등록한다. 그 씬의 배경음악과 설정 화면을 준비한 뒤 World에 명령 목록을 넘겨 판을 준비하게 한다. World가 Start에서 부른다.
        /// </summary>
        /// <param name="world">등록할 World</param>
        /// <param name="musicDefinition">그 씬의 배경음악. 없으면 null</param>
        /// <param name="settingsPresenter">그 씬의 설정 화면. 없으면 null</param>
        public void RegisterWorld(World world, MusicDefinition musicDefinition, SettingsPresenter settingsPresenter)
        {
            if (world == null)
            {
                Debug.LogError("App >> 등록할 World가 null이라 등록하지 않았다.");
                return;
            }

            ReportRemainingScene();
            _world = world;
            BindScene(musicDefinition, settingsPresenter);
            world.Initialize(_sceneCommands, _settingsCommands, _userInterfaceSoundRequests);
        }

        /// <summary>
        /// 등록된 World면 등록을 푼다. World가 OnDestroy에서 부른다.
        /// </summary>
        /// <param name="world">등록을 풀 World</param>
        public void UnregisterWorld(World world)
        {
            // 씬을 바꾸기 전에 App이 먼저 풀었으면 이미 다른 값이다
            if (_world != world)
            {
                return;
            }

            ReleaseScene();
        }

        /// <summary>
        /// Menu 씬의 Menu를 등록한다. 그 씬의 배경음악과 설정 화면을 준비한 뒤 Menu에 명령 목록을 넘겨 Presenter를 연결하게 한다. Menu가 Start에서 부른다.
        /// </summary>
        /// <param name="menu">등록할 Menu</param>
        /// <param name="musicDefinition">그 씬의 배경음악. 없으면 null</param>
        /// <param name="settingsPresenter">그 씬의 설정 화면. 없으면 null</param>
        public void RegisterMenu(Menu menu, MusicDefinition musicDefinition, SettingsPresenter settingsPresenter)
        {
            if (menu == null)
            {
                Debug.LogError("App >> 등록할 Menu가 null이라 등록하지 않았다.");
                return;
            }

            ReportRemainingScene();
            _menu = menu;
            BindScene(musicDefinition, settingsPresenter);
            menu.Initialize(_sceneCommands, _settingsCommands, _userInterfaceSoundRequests);
        }

        /// <summary>
        /// 등록된 Menu면 등록을 푼다. Menu가 OnDestroy에서 부른다.
        /// </summary>
        /// <param name="menu">등록을 풀 Menu</param>
        public void UnregisterMenu(Menu menu)
        {
            // 씬을 바꾸기 전에 App이 먼저 풀었으면 이미 다른 값이다
            if (_menu != menu)
            {
                return;
            }

            ReleaseScene();
        }

        private void Awake()
        {
            // Menu로 돌아오면 Menu 씬에 배치한 App이 하나 더 생긴다. 먼저 있던 App을 남기고 새 App은 없앤다
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            ReadMenuRule();
            LoadVolume();
            _userInterfaceSoundViewContext = CreateUserInterfaceSoundViewContext();
        }

        // Audio Mixer는 Awake에서 값을 무시할 수 있어 볼륨은 Start에서 반영한다
        private void Start()
        {
            if (Instance != this)
            {
                return;
            }

            PresentVolume();
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        // 설정 화면을 연 채로 창을 닫거나 Alt+F4로 끝내면 Close나 Exit 명령 없이 앱이 끝나므로, 여기서 바꾼 볼륨을 저장한다
        private void OnApplicationQuit()
        {
            if (Instance != this)
            {
                return;
            }

            SaveVolume();
        }

        private void FixedUpdate()
        {
            if (Instance != this)
            {
                return;
            }

            if (_world != null)
            {
                _world.Tick();
            }
        }

        private void Update()
        {
            if (Instance != this)
            {
                return;
            }

            if (_world != null)
            {
                _world.ReadInput(_isSettingsOpen, _settingsCommands);
            }
            HandleSettingsCommands(CanOpenSettings());
            // LoadSceneAsync는 부른 프레임에 메인 스레드를 멈출 수 있어, 로딩 화면이 한 번 그려진 다음 프레임에 부른다
            if (_isLoadingRequested && _loading == null && Time.frameCount > _loadingRequestedFrame)
            {
                BeginLoadingWorld();
            }
            // 씬을 바꾸면 지금 씬의 등록을 풀므로 마지막에 둔다
            HandleSceneCommands();
        }

        private void LateUpdate()
        {
            if (Instance != this)
            {
                return;
            }

            if (_world != null)
            {
                // 보스 곡 전환은 적 데이터를 읽어야 해서 World가 배경음악까지 처리한다
                _world.Present(_isSettingsOpen, _musicViewContext);
            }
            else if (_musicViewContext != null)
            {
                MusicViewSystem.Present(_musicViewContext);
            }

            if (_menu != null)
            {
                bool isLoading = _isLoadingRequested;
                float ratio = isLoading ? ReadLoadingRatio() : 0f;
                _menu.Present(isLoading, ratio, ratio >= 1f);
            }

            if (_settingsPresenter != null)
            {
                _settingsPresenter.Present(_isSettingsOpen, in _volume);
            }

            if (_userInterfaceSoundViewContext != null)
            {
                UserInterfaceSoundViewSystem.Present(_userInterfaceSoundRequests, _userInterfaceSoundViewContext);
            }
            _userInterfaceSoundRequests.Clear();
        }

        // 지금 씬에서 설정 화면을 열 수 있는지. World는 일시정지 중에만, Menu는 로딩을 요청하기 전에만 연다
        private bool CanOpenSettings()
        {
            if (_world != null)
            {
                return _world.CanOpenSettings;
            }

            return _menu != null && !_isLoadingRequested;
        }

        // 이전 씬의 등록이 풀리지 않은 채 새 씬이 등록되면 이전 씬의 값이 남는다
        private void ReportRemainingScene()
        {
            if (_world != null || _menu != null)
            {
                Debug.LogWarning("App >> 이전 씬의 World나 Menu 등록이 풀리지 않은 채 새 씬이 등록되어 이전 등록을 버렸다.");
                ReleaseScene();
            }
        }

        // 새 씬의 배경음악을 만들고 설정 화면을 연결한다. 배경음악 오브젝트는 지금 씬에 만들어 씬과 함께 사라진다
        private void BindScene(MusicDefinition musicDefinition, SettingsPresenter settingsPresenter)
        {
            _musicViewContext = CreateMusicViewContext(musicDefinition);
            BindSettingsPresenter(settingsPresenter);
        }

        // 지금 씬의 등록과 씬에 묶인 상태를 비운다. 남은 명령은 이전 씬의 것이라 버린다
        private void ReleaseScene()
        {
            _world = null;
            _menu = null;
            _musicViewContext = null;
            _settingsPresenter = null;
            _isSettingsOpen = false;
            _isLoadingRequested = false;
            _loading = null;
            _sceneCommands.Clear();
            _settingsCommands.Clear();
        }
    }
}
