using System.Collections.Generic;
using TopView.Commands;
using TopView.Components;
using TopView.UserInterface.Views;
using UnityEngine;

namespace TopView.UserInterface.Presenters
{
    // 판이 끝나면 승패와 점수를 담아 GameOverView를 보이고, 재시작과 종료 버튼 입력을 씬 명령으로 넣는다.
    public class GameOverPresenter : MonoBehaviour
    {
        [SerializeField] private GameOverView _view;

        private CommandBuffer _commandBuffer;
        // 버튼 소리를 넣을 목록
        private List<UserInterfaceSoundKind> _soundRequests;
        private bool _isShown;

        /// <summary>
        /// 명령을 넣을 버퍼를 받고 View를 숨긴다. 판을 준비할 때 World가 부른다.
        /// </summary>
        /// <param name="commandBuffer">이번 판의 명령 버퍼</param>
        /// <param name="soundRequests">UI 소리 요청 목록</param>
        public void Initialize(CommandBuffer commandBuffer, List<UserInterfaceSoundKind> soundRequests)
        {
            _commandBuffer = commandBuffer;
            _soundRequests = soundRequests;
            _view.SetVisible(false);
            _isShown = false;
        }

        /// <summary>
        /// 판이 끝나면 승패와 점수를 담아 View를 보인다.
        /// </summary>
        /// <param name="phase">진행 단계</param>
        /// <param name="score">이번 판의 점수</param>
        public void Present(GamePhase phase, int score)
        {
            if (!_isShown && phase != GamePhase.Playing)
            {
                _view.SetResult(phase == GamePhase.Won);
                _view.SetScore(score);
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
            AddSound(UserInterfaceSoundKind.Click);
            AddCommand(SceneCommandType.Restart);
        }

        private void HandleExitClicked()
        {
            AddSound(UserInterfaceSoundKind.Close);
            AddCommand(SceneCommandType.Exit);
        }

        private void AddCommand(SceneCommandType type)
        {
            if (_commandBuffer == null)
            {
                Debug.LogWarning($"GameOverPresenter >> 명령 버퍼가 연결되지 않아 {type} 명령을 버렸다.");
                return;
            }

            _commandBuffer.sceneCommands.Add(new SceneCommand { type = type });
        }

        private void AddSound(UserInterfaceSoundKind kind)
        {
            if (_soundRequests == null)
            {
                Debug.LogWarning($"GameOverPresenter >> 소리 요청 목록이 연결되지 않아 {kind} 소리를 내지 않았다.");
                return;
            }

            _soundRequests.Add(kind);
        }
    }
}
