using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TopView.UserInterface.PlayerHealth
{
    // 플레이어의 체력을 글자와 게이지로 보여 준다.
    public class PlayerHealthView : UserInterfaceView
    {
        [SerializeField] private TMP_Text _healthText;
        [SerializeField] private Image _healthGauge;

        /// <summary>
        /// 체력을 "현재 / 최대" 글자와 게이지로 보여 준다.
        /// </summary>
        /// <param name="current">현재 체력</param>
        /// <param name="max">최대 체력</param>
        public void SetHealth(float current, float max)
        {
            _healthText.text = $"{current:F0} / {max:F0}";
            _healthGauge.rectTransform.anchorMax = new Vector2(current / max, 1f);
        }
    }
}
