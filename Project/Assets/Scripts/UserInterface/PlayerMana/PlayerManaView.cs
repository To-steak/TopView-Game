using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TopView.UserInterface.PlayerMana
{
    // 플레이어의 마나를 글자와 게이지로 보여 준다.
    public class PlayerManaView : UserInterfaceView
    {
        [SerializeField] private TMP_Text _manaText;
        [SerializeField] private Image _manaGauge;

        /// <summary>
        /// 마나를 "현재 / 최대" 글자와 게이지로 보여 준다.
        /// </summary>
        /// <param name="current">현재 마나</param>
        /// <param name="max">최대 마나</param>
        public void SetMana(float current, float max)
        {
            _manaText.text = $"{current} / {max}";
            _manaGauge.fillAmount = current / max;
        }
    }
}
