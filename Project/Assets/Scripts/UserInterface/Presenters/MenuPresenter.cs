using System.Collections.Generic;
using TopView.Commands;
using TopView.Components;
using TopView.UserInterface.Views;
using UnityEngine;

namespace TopView.UserInterface.Presenters
{
    // 로딩 중이 아니면 MenuView를 보이고, 시작·종료 버튼 입력을 씬 명령으로, 설정 버튼 입력을 설정 명령으로 넣는다.
    public class MenuPresenter : MonoBehaviour
    {
        [SerializeField] private MenuView _view;

        private List<SceneCommand> _sceneCommands;
        private List<SettingsCommand> _settingsCommands;
        // 버튼 소리를 넣을 목록
        private List<UserInterfaceSoundKind> _soundRequests;
        private bool _isShown;

        /// <summary>
        /// 명령을 넣을 목록을 받고 View를 보인다. MenuFlow가 App에 등록될 때 부른다.
        /// </summary>
        /// <param name="sceneCommands">씬과 앱 전환 명령 목록</param>
        /// <param name="settingsCommands">설정 명령 목록</param>
        /// <param name="soundRequests">UI 소리 요청 목록</param>
        public void Initialize(List<SceneCommand> sceneCommands, List<SettingsCommand> settingsCommands, List<UserInterfaceSoundKind> soundRequests)
        {
            _sceneCommands = sceneCommands;
            _settingsCommands = settingsCommands;
            _soundRequests = soundRequests;
            _view.SetVisible(true);
            _isShown = true;
        }

        /// <summary>
        /// 보일지 말지가 바뀌었을 때만 View를 켜거나 끈다.
        /// </summary>
        /// <param name="isVisible">메뉴를 보일지</param>
        public void Present(bool isVisible)
        {
            if (isVisible != _isShown)
            {
                _view.SetVisible(isVisible);
                _isShown = isVisible;
            }
        }

        private void OnEnable()
        {
            _view.StartClicked += HandleStartClicked;
            _view.SettingsClicked += HandleSettingsClicked;
            _view.ExitClicked += HandleExitClicked;
        }

        private void OnDisable()
        {
            _view.StartClicked -= HandleStartClicked;
            _view.SettingsClicked -= HandleSettingsClicked;
            _view.ExitClicked -= HandleExitClicked;
        }

        private void HandleStartClicked()
        {
            AddSound(UserInterfaceSoundKind.Click);
            AddSceneCommand(SceneCommandType.LoadWorld);
        }

        private void HandleSettingsClicked()
        {
            AddSound(UserInterfaceSoundKind.Click);
            if (_settingsCommands == null)
            {
                Debug.LogWarning($"MenuPresenter >> 설정 명령 목록이 연결되지 않아 {SettingsCommandType.Open} 명령을 버렸다.");
                return;
            }

            _settingsCommands.Add(new SettingsCommand { type = SettingsCommandType.Open });
        }

        private void HandleExitClicked()
        {
            AddSound(UserInterfaceSoundKind.Close);
            AddSceneCommand(SceneCommandType.Exit);
        }

        private void AddSceneCommand(SceneCommandType type)
        {
            if (_sceneCommands == null)
            {
                Debug.LogWarning($"MenuPresenter >> 씬 명령 목록이 연결되지 않아 {type} 명령을 버렸다.");
                return;
            }

            _sceneCommands.Add(new SceneCommand { type = type });
        }

        private void AddSound(UserInterfaceSoundKind kind)
        {
            if (_soundRequests == null)
            {
                Debug.LogWarning($"MenuPresenter >> 소리 요청 목록이 연결되지 않아 {kind} 소리를 내지 않았다.");
                return;
            }

            _soundRequests.Add(kind);
        }
    }
}
