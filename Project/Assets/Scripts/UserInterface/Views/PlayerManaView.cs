using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TopView.UserInterface.Views
{
    // 플레이어의 마나를 글자와 게이지로 보여 준다. 게이지는 꽉 찬 크기로 두고, 마스크가 오른쪽을 가려 비율만큼만 보인다.
    public class PlayerManaView : UserInterfaceView
    {
        [SerializeField] private TMP_Text _manaText;
        // 게이지 Image의 부모에 붙은 마스크. 오른쪽 여백으로 게이지를 가린다
        [SerializeField] private RectMask2D _manaMask;
        // 게이지가 새 마나 비율까지 부드럽게 움직이는 시간(틱). 0이면 바로 맞춘다
        [SerializeField] private int _tweenTicks = 15;

        // 지금 보이는 게이지 비율(0~1)
        private float _displayedRatio = 1f;
        private DOGetter<float> _getDisplayedRatio;
        private DOSetter<float> _setDisplayedRatio;
        // 지금 게이지를 움직이는 트윈. 끝났거나 끊겼으면 IsActive가 false다
        private TweenerCore<float, float, FloatOptions> _tween;

        /// <summary>
        /// 마나를 "현재 / 최대" 글자와 게이지로 보여 준다.
        /// </summary>
        /// <param name="current">현재 마나</param>
        /// <param name="max">최대 마나</param>
        /// <param name="isInstant">게이지를 연출 없이 바로 맞출지. 줄어드는 연출이 아직 새 비율보다 위에 있으면 끊지 않고 지금 자리에서 새 비율로 이어 간다</param>
        public void SetMana(float current, float max, bool isInstant)
        {
            _manaText.text = $"{current:F0} / {max:F0}";

            float ratio = GetRatio(current, max);
            if (isInstant)
            {
                // 줄어드는 도중에 회복이 시작되면 바로 맞출 때 게이지가 한 번에 건너뛰므로, 진행 중인 트윈의 끝값만 바꾼다. 트윈을 새로 만들지 않는다
                if (_tween != null && _tween.IsActive() && _displayedRatio > ratio)
                {
                    _tween.ChangeEndValue(ratio, true);
                    return;
                }
                _manaMask.DOKill();
                SetDisplayedRatio(ratio);
                return;
            }

            _manaMask.DOKill();

            if (_getDisplayedRatio == null)
            {
                _getDisplayedRatio = GetDisplayedRatio;
                _setDisplayedRatio = SetDisplayedRatio;
            }
            _tween = DOTween.To(_getDisplayedRatio, _setDisplayedRatio, ratio, _tweenTicks * Time.fixedDeltaTime).SetTarget(_manaMask).SetLink(_manaMask.gameObject);
        }

        // 최대값이 0 이하면 나눌 수 없어 빈 게이지로 보인다. 값이 바뀔 때만 불리므로 경고가 매 프레임 쌓이지 않는다
        private static float GetRatio(float current, float max)
        {
            if (max <= 0f)
            {
                Debug.LogWarning($"PlayerManaView >> 최대 마나가 {max}라 게이지를 비워 보인다. 최대 마나는 0보다 커야 한다.");
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
            Vector4 padding = _manaMask.padding;
            padding.z = _manaMask.rectTransform.rect.width * (1f - ratio);
            _manaMask.padding = padding;
        }
    }
}
