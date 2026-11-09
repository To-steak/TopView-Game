using System;
using UnityEngine;
using UnityEngine.UI;

namespace TopView.UserInterface.Views
{
    // 메뉴 화면. 시작, 설정, 종료 버튼 입력을 이벤트로 알린다.
    public class MenuView : UserInterfaceView
    {
        // 시작 버튼을 눌렀다
        public event Action StartClicked;
        // 설정 버튼을 눌렀다
        public event Action SettingsClicked;
        // 종료 버튼을 눌렀다
        public event Action ExitClicked;

        [SerializeField] private Button _startButton;
        [SerializeField] private Button _settingsButton;
        [SerializeField] private Button _exitButton;

        private void OnEnable()
        {
            _startButton.onClick.AddListener(HandleStartButtonClicked);
            _settingsButton.onClick.AddListener(HandleSettingsButtonClicked);
            _exitButton.onClick.AddListener(HandleExitButtonClicked);
        }

        private void OnDisable()
        {
            _startButton.onClick.RemoveListener(HandleStartButtonClicked);
            _settingsButton.onClick.RemoveListener(HandleSettingsButtonClicked);
            _exitButton.onClick.RemoveListener(HandleExitButtonClicked);
        }

        private void HandleStartButtonClicked()
        {
            StartClicked?.Invoke();
        }

        private void HandleSettingsButtonClicked()
        {
            SettingsClicked?.Invoke();
        }

        private void HandleExitButtonClicked()
        {
            ExitClicked?.Invoke();
        }
    }
}
