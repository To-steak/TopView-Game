using System.Collections.Generic;
using TopView.Commands;
using TopView.Components;
using TopView.Definitions;
using TopView.UserInterface.Presenters;
using UnityEngine;

namespace TopView.Core
{
    // Menu 씬에 하나 배치해 메뉴의 화면을 맡는다. App에 등록되면 메뉴 Presenter를 연결하고, App이 부를 때 표시한다.
    // 씬 전환과 로딩, 설정 화면, 배경음악 재생은 App이 한다.
    public class Menu : MonoBehaviour
    {
        [SerializeField] private MenuPresenter _menuPresenter;
        [SerializeField] private LoadingPresenter _loadingPresenter;
        // 설정 화면. 앱 수명의 UI라 Menu는 연결하지 않고, App에 등록할 때 넘긴다
        [SerializeField] private SettingsPresenter _settingsPresenter;
        // Menu 씬의 배경음악 곡과 재생 값. App에 등록할 때 넘긴다
        [SerializeField] private MusicDefinition _musicDefinition;

        /// <summary>
        /// App의 명령 목록을 받아 메뉴 Presenter를 연결한다. App이 Menu를 등록할 때 부른다.
        /// </summary>
        /// <param name="sceneCommands">씬과 앱 전환 명령 목록. App이 소비한다</param>
        /// <param name="settingsCommands">설정 명령 목록. App이 소비한다</param>
        /// <param name="userInterfaceSoundRequests">UI 소리 요청 목록. App이 소비한다</param>
        public void Initialize(List<SceneCommand> sceneCommands, List<SettingsCommand> settingsCommands, List<UserInterfaceSoundKind> userInterfaceSoundRequests)
        {
            if (_menuPresenter == null)
            {
                Debug.LogError("Menu >> Menu Presenter가 연결되어 있지 않다.");
            }
            else
            {
                _menuPresenter.Initialize(sceneCommands, settingsCommands, userInterfaceSoundRequests);
            }

            if (_loadingPresenter == null)
            {
                Debug.LogError("Menu >> Loading Presenter가 연결되어 있지 않다.");
            }
            else
            {
                _loadingPresenter.Initialize(sceneCommands, userInterfaceSoundRequests);
            }
        }

        /// <summary>
        /// 로딩 중이 아니면 메뉴를, 로딩 중이면 로딩 화면과 진행률을 보인다. App이 LateUpdate에서 부른다.
        /// </summary>
        /// <param name="isLoading">World 불러오기를 요청했는지</param>
        /// <param name="loadingRatio">진행 비율(0~1)</param>
        /// <param name="canEnter">불러오기와 최소 시간이 끝나 들어갈 수 있는지</param>
        public void Present(bool isLoading, float loadingRatio, bool canEnter)
        {
            if (_menuPresenter != null)
            {
                _menuPresenter.Present(!isLoading);
            }
            if (_loadingPresenter != null)
            {
                _loadingPresenter.Present(isLoading, loadingRatio, canEnter);
            }
        }

        // 씬의 다른 오브젝트가 Awake를 마친 뒤에 App에 등록한다. App이 Initialize를 불러 Presenter를 연결한다
        private void Start()
        {
            App app = App.Instance;
            if (app == null)
            {
                Debug.LogError("Menu >> App이 없어 메뉴를 연결하지 않았다. Menu 씬에 App 프리팹이 배치되어 있는지 확인한다.");
                return;
            }

            app.RegisterMenu(this, _musicDefinition, _settingsPresenter);
        }

        private void OnDestroy()
        {
            App app = App.Instance;
            if (app != null)
            {
                app.UnregisterMenu(this);
            }
        }
    }
}
