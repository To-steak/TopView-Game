using TopView.Commands;
using TopView.Components;
using UnityEngine;

namespace TopView.UserInterface.GameOver
{
    // 게임오버가 되면 GameOverView를 보이고, 재시작과 종료 버튼 입력을 판 흐름 명령으로 넣는다.
    public class GameOverPresenter : MonoBehaviour
    {
        [SerializeField] private GameOverView _view;

        private CommandBuffer _commandBuffer;
        private bool _isShown;

        /// <summary>
        /// 명령을 넣을 버퍼를 받고 View를 숨긴다. 판을 준비할 때 Runner가 부른다.
        /// </summary>
        /// <param name="commandBuffer">이번 판의 명령 버퍼</param>
        public void Initialize(CommandBuffer commandBuffer)
        {
            _commandBuffer = commandBuffer;
            _view.SetVisible(false);
            _isShown = false;
        }

        /// <summary>
        /// 게임오버가 되면 View를 보인다.
        /// </summary>
        /// <param name="phase">진행 단계</param>
        public void Present(GamePhase phase)
        {
            if (!_isShown && phase == GamePhase.Over)
            {
                _view.SetVisible(true);
                _isShown = true;
            }
        }

        private void OnEnable()
        {
            _view.RestartClicked += HandleRestartClicked;
            _view.ExitClicked += HandleExitClicked;
        }

        private void OnDisable()
        {
            _view.RestartClicked -= HandleRestartClicked;
            _view.ExitClicked -= HandleExitClicked;
        }

        private void HandleRestartClicked()
        {
            AddCommand(GameFlowCommandType.Restart);
        }

        private void HandleExitClicked()
        {
            AddCommand(GameFlowCommandType.Exit);
        }

        private void AddCommand(GameFlowCommandType type)
        {
            if (_commandBuffer == null)
            {
                Debug.LogWarning($"GameOverPresenter >> 명령 버퍼가 연결되지 않아 {type} 명령을 버렸다.");
                return;
            }

            _commandBuffer.gameFlowCommands.Add(new GameFlowCommand { type = type });
        }
    }
}
