using DG.Tweening;
using DG.Tweening.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TopView.UserInterface.Views
{
    // 플레이어의 체력을 글자와 게이지로 보여 준다. 게이지는 꽉 찬 크기로 두고, 마스크가 오른쪽을 가려 비율만큼만 보인다.
    public class PlayerHealthView : UserInterfaceView
    {
        [SerializeField] private TMP_Text _healthText;
        // 게이지 Image의 부모에 붙은 마스크. 오른쪽 여백으로 게이지를 가린다
        [SerializeField] private RectMask2D _healthMask;
        // 게이지가 새 체력 비율까지 부드럽게 움직이는 시간(틱). 0이면 바로 맞춘다
        [SerializeField] private int _tweenTicks = 15;

        // 지금 보이는 게이지 비율(0~1)
        private float _displayedRatio = 1f;
        private DOGetter<float> _getDisplayedRatio;
        private DOSetter<float> _setDisplayedRatio;

        /// <summary>
        /// 체력을 "현재 / 최대" 글자와 게이지로 보여 준다.
        /// </summary>
        /// <param name="current">현재 체력</param>
        /// <param name="max">최대 체력</param>
        /// <param name="isInstant">게이지를 연출 없이 바로 맞출지</param>
        public void SetHealth(float current, float max, bool isInstant)
        {
            // 반올림하면 1 미만으로 남은 살아 있는 플레이어가 0으로 보이므로, 체력이 남아 있으면 1 이상으로 보인다
            int shownHealth = current > 0f ? Mathf.Max(1, Mathf.RoundToInt(current)) : 0;
            _healthText.text = $"{shownHealth} / {max:F0}";

            float ratio = GetRatio(current, max);
            _healthMask.DOKill();
            if (isInstant)
            {
                SetDisplayedRatio(ratio);
                return;
            }

            if (_getDisplayedRatio == null)
            {
                _getDisplayedRatio = GetDisplayedRatio;
                _setDisplayedRatio = SetDisplayedRatio;
            }
            DOTween.To(_getDisplayedRatio, _setDisplayedRatio, ratio, _tweenTicks * Time.fixedDeltaTime).SetTarget(_healthMask).SetLink(_healthMask.gameObject);
        }

        // 최대값이 0 이하면 나눌 수 없어 빈 게이지로 보인다. 값이 바뀔 때만 불리므로 경고가 매 프레임 쌓이지 않는다
        private static float GetRatio(float current, float max)
        {
            if (max <= 0f)
            {
                Debug.LogWarning($"PlayerHealthView >> 최대 체력이 {max}라 게이지를 비워 보인다. 최대 체력은 0보다 커야 한다.");
                return 0f;
            }
            return current / max;
        }

        private float GetDisplayedRatio()
        {
            return _displayedRatio;
        }

        // 비율을 기억하고, 마스크의 오른쪽 여백을 가릴 폭으로 맞춘다.
        private void SetDisplayedRatio(float ratio)
        {
            _displayedRatio = ratio;
            Vector4 padding = _healthMask.padding;
            padding.z = _healthMask.rectTransform.rect.width * (1f - ratio);
            _healthMask.padding = padding;
        }
    }
}
