using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TopView.UserInterface.Views
{
    // 판이 끝난 화면. 승패를 표시하고, 재시작과 종료 버튼 입력을 이벤트로 알린다.
    public class GameOverView : UserInterfaceView
    {
        // 재시작 버튼을 눌렀다
        public event Action RestartClicked;
        // 종료 버튼을 눌렀다
        public event Action ExitClicked;

        [SerializeField] private Button _restartButton;
        [SerializeField] private Button _exitButton;
        [SerializeField] private TMP_Text _resultText;
        [SerializeField] private string _victoryText = "Victory";
        [SerializeField] private string _defeatText = "Defeat";

        /// <summary>
        /// 승패 문구를 표시한다.
        /// </summary>
        /// <param name="isVictory">승리했으면 true</param>
        public void SetResult(bool isVictory)
        {
            _resultText.text = isVictory ? _victoryText : _defeatText;
        }

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
