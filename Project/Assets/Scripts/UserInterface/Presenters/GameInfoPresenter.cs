using TopView.Commands;
using TopView.Components;
using TopView.UserInterface.Views;
using UnityEngine;

namespace TopView.UserInterface.Presenters
{
    // GameInfoView의 버튼 입력을 판 흐름 명령으로 넣고, Runner가 매 프레임 넘기는 진행 상태를 View에 보여 준다.
    public class GameInfoPresenter : MonoBehaviour
    {
        [SerializeField] private GameInfoView view;

        private CommandBuffer _commandBuffer;
        // 같은 값을 매 프레임 다시 쓰지 않도록 마지막으로 보여 준 값을 기억한다
        private GamePhase _presentedPhase;
        private int _presentedSeconds;
        private int _presentedScore;

        private const int NOT_PRESENTED = -1;

        /// <summary>
        /// 명령을 넣을 버퍼를 받고, 보여 준 값을 처음 상태로 되돌린다. 판을 준비할 때 Runner가 부른다.
        /// </summary>
        /// <param name="commandBuffer">이번 판의 명령 버퍼</param>
        public void Initialize(CommandBuffer commandBuffer)
        {
            _commandBuffer = commandBuffer;
            _presentedPhase = GamePhase.Ready;
            _presentedSeconds = NOT_PRESENTED;
            _presentedScore = NOT_PRESENTED;
        }

        /// <summary>
        /// 진행 상태를 View에 보여 준다. 시작 전에는 씬에 놓인 그대로 두고, 시작한 뒤부터 시간과 점수를 갱신한다.
        /// </summary>
        /// <param name="phase">진행 단계</param>
        /// <param name="remainingTicks">남은 시간(틱)</param>
        /// <param name="score">점수</param>
        public void Present(GamePhase phase, int remainingTicks, int score)
        {
            if (phase == GamePhase.Ready)
            {
                return;
            }

            if (_presentedPhase == GamePhase.Ready)
            {
                view.HideGameStartPanel();
            }

            if (phase == GamePhase.Over && _presentedPhase != GamePhase.Over)
            {
                view.ShowGameOverPanel();
            }
            _presentedPhase = phase;

            int ticksPerSecond = Mathf.RoundToInt(1f / Time.fixedDeltaTime);
            int seconds = remainingTicks / ticksPerSecond;
            if (seconds != _presentedSeconds)
            {
                view.SetTime(seconds);
                _presentedSeconds = seconds;
            }

            if (score != _presentedScore)
            {
                view.SetScore(score);
                _presentedScore = score;
            }
        }

        private void OnEnable()
        {
            view.StartClicked += HandleStartClicked;
            view.RestartClicked += HandleRestartClicked;
            view.ExitClicked += HandleExitClicked;
        }

        private void OnDisable()
        {
            view.StartClicked -= HandleStartClicked;
            view.RestartClicked -= HandleRestartClicked;
            view.ExitClicked -= HandleExitClicked;
        }

        private void HandleStartClicked()
        {
            AddCommand(GameFlowCommandType.Start);
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
                Debug.LogWarning($"GameInfoPresenter >> 명령 버퍼가 연결되지 않아 {type} 명령을 버렸다.");
                return;
            }

            _commandBuffer.gameFlowCommands.Add(new GameFlowCommand { type = type });
        }
    }
}
