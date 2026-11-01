using TopView.Commands;
using TopView.Components;
using UnityEngine;

namespace TopView.UserInterface.GameStart
{
    // 판이 시작되기 전까지 GameStartView를 보이고, 시작 버튼 입력을 판 흐름 명령으로 넣는다.
    public class GameStartPresenter : MonoBehaviour
    {
        [SerializeField] private GameStartView _view;

        private CommandBuffer _commandBuffer;
        private bool _isShown;

        /// <summary>
        /// 명령을 넣을 버퍼를 받고 View를 보인다. 판을 준비할 때 Runner가 부른다.
        /// </summary>
        /// <param name="commandBuffer">이번 판의 명령 버퍼</param>
        public void Initialize(CommandBuffer commandBuffer)
        {
            _commandBuffer = commandBuffer;
            _view.SetVisible(true);
            _isShown = true;
        }

        /// <summary>
        /// 판이 시작되면 View를 숨긴다.
        /// </summary>
        /// <param name="phase">진행 단계</param>
        public void Present(GamePhase phase)
        {
            if (_isShown && phase != GamePhase.Ready)
            {
                _view.SetVisible(false);
                _isShown = false;
            }
        }

        private void OnEnable()
        {
            _view.StartClicked += HandleStartClicked;
        }

        private void OnDisable()
        {
            _view.StartClicked -= HandleStartClicked;
        }

        private void HandleStartClicked()
        {
            if (_commandBuffer == null)
            {
                Debug.LogWarning("GameStartPresenter >> 명령 버퍼가 연결되지 않아 Start 명령을 버렸다.");
                return;
            }

            _commandBuffer.gameFlowCommands.Add(new GameFlowCommand { type = GameFlowCommandType.Start });
        }
    }
}
