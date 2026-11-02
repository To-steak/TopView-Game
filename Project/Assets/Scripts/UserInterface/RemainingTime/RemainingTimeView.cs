using TMPro;
using UnityEngine;

namespace TopView.UserInterface.RemainingTime
{
    // 남은 시간을 "분 : 초"로 보여 준다.
    public class RemainingTimeView : UserInterfaceView
    {
        [SerializeField] private TMP_Text _timeText;

        private const int SECONDS_PER_MINUTE = 60;

        /// <summary>
        /// 남은 시간을 "분 : 초"로 보여 준다.
        /// </summary>
        /// <param name="seconds">남은 시간(초)</param>
        public void SetTime(int seconds)
        {
            int minutes = seconds / SECONDS_PER_MINUTE;
            int remainderSeconds = seconds % SECONDS_PER_MINUTE;
            _timeText.text = $"{minutes:00} : {remainderSeconds:00}";
        }
    }
}
