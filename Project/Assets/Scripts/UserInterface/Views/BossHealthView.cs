using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace TopView.UserInterface.Views
{
    // 보스 체력 게이지를 보여 준다.
    public class BossHealthView : UserInterfaceView
    {
        [FormerlySerializedAs("hPGauge")]
        [SerializeField] private Image healthGauge;

        /// <summary>
        /// 게이지를 현재 체력 / 최대 체력만큼 채운다.
        /// </summary>
        /// <param name="current">현재 체력</param>
        /// <param name="max">최대 체력</param>
        public void SetHealth(float current, float max)
        {
            healthGauge.fillAmount = current / max;
        }
    }
}
