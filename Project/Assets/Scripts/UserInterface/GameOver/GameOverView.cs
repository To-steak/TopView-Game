using System;
using UnityEngine;
using UnityEngine.UI;

namespace TopView.UserInterface.GameOver
{
    // 게임오버 화면. 재시작과 종료 버튼 입력을 이벤트로 알린다.
    public class GameOverView : UserInterfaceView
    {
        // 재시작 버튼을 눌렀다
        public event Action RestartClicked;
        // 종료 버튼을 눌렀다
        public event Action ExitClicked;

        [SerializeField] private Button _restartButton;
        [SerializeField] private Button _exitButton;

        private void OnEnable()
        {
            _restartButton.onClick.AddListener(HandleRestartButtonClicked);
            _exitButton.onClick.AddListener(HandleExitButtonClicked);
        }

        private void OnDisable()
        {
            _restartButton.onClick.RemoveListener(HandleRestartButtonClicked);
            _exitButton.onClick.RemoveListener(HandleExitButtonClicked);
        }

        private void HandleRestartButtonClicked()
        {
            RestartClicked?.Invoke();
        }

        private void HandleExitButtonClicked()
        {
            ExitClicked?.Invoke();
        }
    }
}
