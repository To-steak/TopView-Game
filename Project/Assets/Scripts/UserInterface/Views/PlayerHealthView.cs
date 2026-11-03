using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TopView.UserInterface.Views
{
    // 플레이어의 체력을 글자와 게이지로 보여 준다.
    public class PlayerHealthView : UserInterfaceView
    {
        [SerializeField] private TMP_Text _healthText;
        [SerializeField] private Image _healthGauge;
        // 게이지가 새 체력 비율까지 부드럽게 움직이는 시간(틱). 0이면 바로 맞춘다
        [SerializeField] private int _tweenTicks = 15;

        /// <summary>
        /// 체력을 "현재 / 최대" 글자와 게이지로 보여 준다.
        /// </summary>
        /// <param name="current">현재 체력</param>
        /// <param name="max">최대 체력</param>
        /// <param name="isInstant">게이지를 연출 없이 바로 맞출지</param>
        public void SetHealth(float current, float max, bool isInstant)
        {
            _healthText.text = $"{current:F0} / {max:F0}";

            RectTransform gaugeTransform = _healthGauge.rectTransform;
            Vector2 anchorMax = new Vector2(current / max, 1f);
            gaugeTransform.DOKill();
            if (isInstant)
            {
                gaugeTransform.anchorMax = anchorMax;
            }
            else
            {
                gaugeTransform.DOAnchorMax(anchorMax, _tweenTicks * Time.fixedDeltaTime).SetLink(_healthGauge.gameObject);
            }
        }
    }
}
