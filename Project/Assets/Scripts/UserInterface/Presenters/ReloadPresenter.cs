using TopView.UserInterface.Views;
using UnityEngine;

namespace TopView.UserInterface.Presenters
{
    // 플레이어가 재장전하는 동안 ReloadView를 보이고, Runner가 매 프레임 넘기는 재장전 진행을 보여 준다.
    public class ReloadPresenter : MonoBehaviour
    {
        [SerializeField] private ReloadView _view;

        private bool _isShown;
        // 같은 값을 매 프레임 다시 쓰지 않도록 마지막으로 보여 준 값을 기억한다
        private int _presentedElapsedTicks;
        private int _presentedReloadTicks;

        /// <summary>
        /// View를 연출 없이 바로 숨긴다. 판을 준비할 때 Runner가 부른다.
        /// </summary>
        public void Initialize()
        {
            _view.HideImmediately();
            _isShown = false;
        }

        /// <summary>
        /// 재장전 중이면 View를 페이드 인하고 진행률을 보여 준다. 재장전이 끝나거나 취소되면 페이드 아웃한다.
        /// </summary>
        /// <param name="isReloading">살아 있는 플레이어가 재장전 상태인지</param>
        /// <param name="elapsedTicks">재장전에 들어간 뒤 지난 시간(틱)</param>
        /// <param name="reloadTicks">재장전에 걸리는 시간(틱)</param>
        public void Present(bool isReloading, int elapsedTicks, int reloadTicks)
        {
            if (!isReloading)
            {
                if (_isShown)
                {
                    _view.FadeOut();
                    _isShown = false;
                }
                return;
            }

            bool isFirstFrame = !_isShown;
            if (isFirstFrame)
            {
                if (reloadTicks <= 0)
                {
                    Debug.LogWarning($"ReloadPresenter >> Reload Ticks가 {reloadTicks}라 진행률을 꽉 찬 것으로 보여 준다.");
                }
                _view.FadeIn();
                _isShown = true;
            }

            if (isFirstFrame || elapsedTicks != _presentedElapsedTicks || reloadTicks != _presentedReloadTicks)
            {
                float ratio = reloadTicks > 0 ? Mathf.Clamp01((float)elapsedTicks / reloadTicks) : 1f;
                _view.SetProgress(ratio);
                _presentedElapsedTicks = elapsedTicks;
                _presentedReloadTicks = reloadTicks;
            }
        }
    }
}
