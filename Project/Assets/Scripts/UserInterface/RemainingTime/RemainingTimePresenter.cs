using TopView.Components;
using UnityEngine;

namespace TopView.UserInterface.RemainingTime
{
    // 판이 시작된 뒤부터 Runner가 매 프레임 넘기는 남은 시간을 초 단위로 보여 준다.
    public class RemainingTimePresenter : MonoBehaviour
    {
        [SerializeField] private RemainingTimeView _view;

        // 같은 값을 매 프레임 다시 쓰지 않도록 마지막으로 보여 준 값을 기억한다
        private int _presentedSeconds;

        private const int NOT_PRESENTED = -1;

        /// <summary>
        /// 보여 준 값을 처음 상태로 되돌린다. 판을 준비할 때 Runner가 부른다.
        /// </summary>
        public void Initialize()
        {
            _presentedSeconds = NOT_PRESENTED;
        }

        /// <summary>
        /// 시작 전에는 씬에 놓인 그대로 두고, 시작한 뒤부터 남은 시간을 갱신한다.
        /// </summary>
        /// <param name="phase">진행 단계</param>
        /// <param name="remainingTicks">남은 시간(틱)</param>
        public void Present(GamePhase phase, int remainingTicks)
        {
            if (phase == GamePhase.Ready)
            {
                return;
            }

            int ticksPerSecond = Mathf.RoundToInt(1f / Time.fixedDeltaTime);
            int seconds = remainingTicks / ticksPerSecond;
            if (seconds != _presentedSeconds)
            {
                _view.SetTime(seconds);
                _presentedSeconds = seconds;
            }
        }
    }
}
