using UnityEngine;

namespace TopView.UserInterface.RemainingTime
{
    // Runner가 매 프레임 넘기는 남은 시간을 초 단위로 보여 준다.
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
        /// 남은 시간을 초 단위로 바꿔, 바뀌었을 때만 View에 보여 준다.
        /// </summary>
        /// <param name="remainingTicks">남은 시간(틱)</param>
        public void Present(int remainingTicks)
        {
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
