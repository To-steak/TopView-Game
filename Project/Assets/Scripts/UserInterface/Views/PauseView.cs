using System;
using UnityEngine;
using UnityEngine.UI;

namespace TopView.UserInterface.Views
{
    // 일시정지 화면. 계속, 탈출, 설정, 나가기 버튼 입력을 이벤트로 알린다.
    public class PauseView : UserInterfaceView
    {
        // 계속 버튼을 눌렀다
        public event Action ResumeClicked;
        // 탈출 버튼을 눌렀다
        public event Action UnstuckClicked;
        // 설정 버튼을 눌렀다
        public event Action SettingsClicked;
        // 나가기 버튼을 눌렀다
        public event Action ExitClicked;

        [SerializeField] private Button _resumeButton;
        [SerializeField] private Button _unstuckButton;
        [SerializeField] private Button _settingsButton;
        [SerializeField] private Button _exitButton;

        private void OnEnable()
        {
            _resumeButton.onClick.AddListener(HandleResumeButtonClicked);
            _unstuckButton.onClick.AddListener(HandleUnstuckButtonClicked);
            _settingsButton.onClick.AddListener(HandleSettingsButtonClicked);
            _exitButton.onClick.AddListener(HandleExitButtonClicked);
        }

        private void OnDisable()
        {
            _resumeButton.onClick.RemoveListener(HandleResumeButtonClicked);
            _unstuckButton.onClick.RemoveListener(HandleUnstuckButtonClicked);
            _settingsButton.onClick.RemoveListener(HandleSettingsButtonClicked);
            _exitButton.onClick.RemoveListener(HandleExitButtonClicked);
        }

        private void HandleResumeButtonClicked()
        {
            ResumeClicked?.Invoke();
        }

        private void HandleUnstuckButtonClicked()
        {
            UnstuckClicked?.Invoke();
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
