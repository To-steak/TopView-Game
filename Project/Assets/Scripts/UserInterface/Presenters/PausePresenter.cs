using System.Collections.Generic;
using TopView.Commands;
using TopView.Components;
using TopView.UserInterface.Views;
using UnityEngine;

namespace TopView.UserInterface.Presenters
{
    // 일시정지 중이면 PauseView를 보이고, 계속·설정·나가기 버튼 입력을 판 흐름 명령으로 넣는다.
    public class PausePresenter : MonoBehaviour
    {
        [SerializeField] private PauseView _view;

        private CommandBuffer _commandBuffer;
        // 버튼 소리를 넣을 목록
        private List<UserInterfaceSoundKind> _soundRequests;
        private bool _isShown;

        /// <summary>
        /// 명령을 넣을 버퍼를 받고 View를 숨긴다. 판을 준비할 때 Runner가 부른다.
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
        /// 보일지 말지가 바뀌었을 때만 View를 켜거나 끈다.
        /// </summary>
        /// <param name="isVisible">일시정지 화면을 보일지</param>
        public void Present(bool isVisible)
        {
            if (isVisible != _isShown)
            {
                _view.SetVisible(isVisible);
                _isShown = isVisible;
            }
        }

        private void OnEnable()
        {
            _view.ResumeClicked += HandleResumeClicked;
            _view.SettingsClicked += HandleSettingsClicked;
            _view.ExitClicked += HandleExitClicked;
        }

        private void OnDisable()
        {
            _view.ResumeClicked -= HandleResumeClicked;
            _view.SettingsClicked -= HandleSettingsClicked;
            _view.ExitClicked -= HandleExitClicked;
        }

        private void HandleResumeClicked()
        {
            AddSound(UserInterfaceSoundKind.Click);
            AddCommand(GameFlowCommandType.Resume);
        }

        private void HandleSettingsClicked()
        {
            AddSound(UserInterfaceSoundKind.Click);
            AddCommand(GameFlowCommandType.OpenSettings);
        }

        private void HandleExitClicked()
        {
            AddSound(UserInterfaceSoundKind.Close);
            AddCommand(GameFlowCommandType.ReturnToMenu);
        }

        private void AddCommand(GameFlowCommandType type)
        {
            if (_commandBuffer == null)
            {
                Debug.LogWarning($"PausePresenter >> 명령 버퍼가 연결되지 않아 {type} 명령을 버렸다.");
                return;
            }

            _commandBuffer.gameFlowCommands.Add(new GameFlowCommand { type = type });
        }

        private void AddSound(UserInterfaceSoundKind kind)
        {
            if (_soundRequests == null)
            {
                Debug.LogWarning($"PausePresenter >> 소리 요청 목록이 연결되지 않아 {kind} 소리를 내지 않았다.");
                return;
            }

            _soundRequests.Add(kind);
        }
    }
}
