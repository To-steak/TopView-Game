using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TopView.UserInterface.Views
{
    // 남은 시간, 점수, 시작 화면, 게임오버 화면을 보여 주고 버튼 입력을 이벤트로 알린다.
    public class GameInfoView : MonoBehaviour
    {
        // 시작 버튼을 눌렀다
        public event Action StartClicked;
        // 재시작 버튼을 눌렀다
        public event Action RestartClicked;
        // 종료 버튼을 눌렀다
        public event Action ExitClicked;

        [SerializeField] private TMP_Text timeText;
        [SerializeField] private TMP_Text scoreText;
        [SerializeField] private GameObject gameoverPanel;
        [SerializeField] private GameObject gameStartPanel;
        [SerializeField] private Button startButton;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button exitButton;

        private const int SECONDS_PER_MINUTE = 60;

        /// <summary>
        /// 남은 시간을 "분 : 초"로 보여 준다.
        /// </summary>
        /// <param name="seconds">남은 시간(초)</param>
        public void SetTime(int seconds)
        {
            int minutes = seconds / SECONDS_PER_MINUTE;
            int remainderSeconds = seconds % SECONDS_PER_MINUTE;
            timeText.text = $"{minutes:00} : {remainderSeconds:00}";
        }

        /// <summary>
        /// 점수를 천 단위 구분 기호와 함께 보여 준다.
        /// </summary>
        /// <param name="score">점수</param>
        public void SetScore(int score)
        {
            scoreText.text = score.ToString("N0");
        }

        /// <summary>
        /// 게임오버 화면을 켠다.
        /// </summary>
        public void ShowGameOverPanel()
        {
            gameoverPanel.SetActive(true);
        }

        /// <summary>
        /// 시작 화면을 끈다.
        /// </summary>
        public void HideGameStartPanel()
        {
            gameStartPanel.SetActive(false);
        }

        private void OnEnable()
        {
            startButton.onClick.AddListener(HandleStartButtonClicked);
            restartButton.onClick.AddListener(HandleRestartButtonClicked);
            exitButton.onClick.AddListener(HandleExitButtonClicked);
        }

        private void OnDisable()
        {
            startButton.onClick.RemoveListener(HandleStartButtonClicked);
            restartButton.onClick.RemoveListener(HandleRestartButtonClicked);
            exitButton.onClick.RemoveListener(HandleExitButtonClicked);
        }

        private void HandleStartButtonClicked()
        {
            StartClicked?.Invoke();
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
