using System;
using UnityEngine;
using UnityEngine.UI;

namespace TopView.UserInterface.GameStart
{
    // 시작 화면. 시작 버튼 입력을 이벤트로 알린다.
    public class GameStartView : UserInterfaceView
    {
        // 시작 버튼을 눌렀다
        public event Action StartClicked;

        [SerializeField] private Button _startButton;

        private void OnEnable()
        {
            _startButton.onClick.AddListener(HandleStartButtonClicked);
        }

        private void OnDisable()
        {
            _startButton.onClick.RemoveListener(HandleStartButtonClicked);
        }

        private void HandleStartButtonClicked()
        {
            StartClicked?.Invoke();
        }
    }
}
