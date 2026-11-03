using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TopView.UserInterface.Views
{
    // 플레이어의 마나를 글자와 게이지로 보여 준다.
    public class PlayerManaView : UserInterfaceView
    {
        [SerializeField] private TMP_Text _manaText;
        [SerializeField] private Image _manaGauge;
        // 게이지가 새 마나 비율까지 부드럽게 움직이는 시간(틱). 0이면 바로 맞춘다
        [SerializeField] private int _tweenTicks = 15;

        /// <summary>
        /// 마나를 "현재 / 최대" 글자와 게이지로 보여 준다.
        /// </summary>
        /// <param name="current">현재 마나</param>
        /// <param name="max">최대 마나</param>
        /// <param name="isInstant">게이지를 연출 없이 바로 맞출지</param>
        public void SetMana(float current, float max, bool isInstant)
        {
            _manaText.text = $"{current:F0} / {max:F0}";

            RectTransform gaugeTransform = _manaGauge.rectTransform;
            Vector2 anchorMax = new Vector2(current / max, 1f);
            gaugeTransform.DOKill();
            if (isInstant)
            {
                gaugeTransform.anchorMax = anchorMax;
            }
            else
            {
                gaugeTransform.DOAnchorMax(anchorMax, _tweenTicks * Time.fixedDeltaTime).SetLink(_manaGauge.gameObject);
            }
        }
    }
}
