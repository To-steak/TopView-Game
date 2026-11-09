using System.Collections.Generic;
using TopView.Commands;
using TopView.UserInterface.Views;
using UnityEngine;

namespace TopView.UserInterface.Presenters
{
    // 씬을 불러오는 동안 LoadingView를 보이고, MenuFlow가 매 프레임 넘기는 진행률을 보여 준다.
    // 들어갈 준비가 되면 들어가기 버튼을 누를 수 있게 하고, 버튼 입력을 메뉴 명령으로 넣는다.
    public class LoadingPresenter : MonoBehaviour
    {
        [SerializeField] private LoadingView _view;

        private List<MenuCommand> _commands;
        private bool _isShown;
        // 같은 값을 매 프레임 다시 쓰지 않도록 마지막으로 보여 준 값을 기억한다
        private float _presentedRatio;
        private bool _presentedCanEnter;

        /// <summary>
        /// 명령을 넣을 목록을 받고 View를 숨긴다. MenuFlow가 시작할 때 부른다.
        /// </summary>
        /// <param name="commands">메뉴 명령 목록</param>
        public void Initialize(List<MenuCommand> commands)
        {
            _commands = commands;
            _view.SetVisible(false);
            _isShown = false;
        }

        /// <summary>
        /// 불러오는 중이면 View를 보이고 진행률을 보여 준다. 들어갈 준비가 되면 들어가기 버튼을 누를 수 있게 한다.
        /// </summary>
        /// <param name="isLoading">씬을 불러오는 중인지</param>
        /// <param name="ratio">진행 비율(0~1)</param>
        /// <param name="canEnter">불러오기와 최소 시간이 끝나 들어갈 수 있는지</param>
        public void Present(bool isLoading, float ratio, bool canEnter)
        {
            if (!isLoading)
            {
                if (_isShown)
                {
                    _view.SetVisible(false);
                    _isShown = false;
                }
                return;
            }

            bool isFirstFrame = !_isShown;
            if (isFirstFrame)
            {
                _view.SetVisible(true);
                _isShown = true;
            }

            if (isFirstFrame || ratio != _presentedRatio)
            {
                _view.SetProgress(ratio);
                _presentedRatio = ratio;
            }

            if (isFirstFrame || canEnter != _presentedCanEnter)
            {
                _view.SetEnterButtonInteractable(canEnter);
                _presentedCanEnter = canEnter;
            }
        }

        private void OnEnable()
        {
            _view.EnterClicked += HandleEnterClicked;
        }

        private void OnDisable()
        {
            _view.EnterClicked -= HandleEnterClicked;
        }

        private void HandleEnterClicked()
        {
            if (_commands == null)
            {
                Debug.LogWarning($"LoadingPresenter >> 명령 목록이 연결되지 않아 {MenuCommandType.EnterWorld} 명령을 버렸다.");
                return;
            }

            _commands.Add(new MenuCommand { type = MenuCommandType.EnterWorld });
        }
    }
}
