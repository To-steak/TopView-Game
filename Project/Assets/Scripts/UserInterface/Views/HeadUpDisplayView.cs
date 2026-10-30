using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace TopView.UserInterface.Views
{
    // 플레이어의 체력과 탄약을 보여 준다.
    public class HeadUpDisplayView : MonoBehaviour
    {
        [SerializeField] private TMP_Text healthText;
        [SerializeField] private TMP_Text ammoText;
        [FormerlySerializedAs("hPGauge")]
        [SerializeField] private Image healthGauge;

        /// <summary>
        /// 체력을 "현재 / 최대" 글자와 게이지로 보여 준다.
        /// </summary>
        /// <param name="current">현재 체력</param>
        /// <param name="max">최대 체력</param>
        public void SetHealth(float current, float max)
        {
            healthText.text = $"{current} / {max}";
            healthGauge.fillAmount = current / max;
        }

        /// <summary>
        /// 탄약 글자를 보여 준다.
        /// </summary>
        /// <param name="text">보여 줄 글자</param>
        public void SetAmmoText(string text)
        {
            ammoText.text = text;
        }
    }
}
