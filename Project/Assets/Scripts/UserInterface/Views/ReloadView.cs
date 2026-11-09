using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TopView.UserInterface.Views
{
    // 재장전 진행을 글자(0~100%)와 게이지로 보여 준다. 게이지는 꽉 찬 크기로 두고, 마스크가 오른쪽을 가려 비율만큼만 보인다.
    // 보이고 숨길 때 Content의 CanvasGroup 투명도를 페이드한다.
    public class ReloadView : UserInterfaceView
    {
        [SerializeField] private TMP_Text _progressText;
        // 게이지 Image의 부모에 붙은 마스크. 오른쪽 여백으로 게이지를 가린다
        [SerializeField] private RectMask2D _progressMask;
        // Content에 붙은 CanvasGroup. 투명도로 페이드한다
        [SerializeField] private CanvasGroup _contentGroup;
        // 페이드 인/아웃에 걸리는 시간(틱). 0이면 바로 맞춘다
        [SerializeField] private int _fadeTicks = 10;

        private TweenCallback _deactivateContent;

        /// <summary>
        /// 재장전 진행률을 글자와 게이지로 보여 준다.
        /// </summary>
        /// <param name="ratio">진행 비율(0~1)</param>
        public void SetProgress(float ratio)
        {
            // SetText의 서식 인자는 문자열을 새로 만들지 않아 매 프레임 불러도 할당이 없다
            _progressText.SetText("재장전 {0}%", Mathf.FloorToInt(ratio * 100f));

            Vector4 padding = _progressMask.padding;
            padding.z = _progressMask.rectTransform.rect.width * (1f - ratio);
            _progressMask.padding = padding;
        }

        /// <summary>
        /// Content를 켜고 투명도를 1까지 올린다. 페이드 아웃 중이면 그 투명도에서 이어서 올린다.
        /// </summary>
        public void FadeIn()
        {
            _contentGroup.DOKill();
            if (!content.gameObject.activeSelf)
            {
                _contentGroup.alpha = 0f;
                SetVisible(true);
            }
            _contentGroup.DOFade(1f, _fadeTicks * Time.fixedDeltaTime).SetLink(_contentGroup.gameObject);
        }

        /// <summary>
        /// 투명도를 0까지 내린 뒤 Content를 끈다.
        /// </summary>
        public void FadeOut()
        {
            if (_deactivateContent == null)
            {
                _deactivateContent = DeactivateContent;
            }
            _contentGroup.DOKill();
            _contentGroup.DOFade(0f, _fadeTicks * Time.fixedDeltaTime).SetLink(_contentGroup.gameObject).OnComplete(_deactivateContent);
        }

        /// <summary>
        /// 진행 중인 페이드를 멈추고 연출 없이 바로 숨긴다.
        /// </summary>
        public void HideImmediately()
        {
            _contentGroup.DOKill();
            _contentGroup.alpha = 0f;
            SetVisible(false);
        }

        private void DeactivateContent()
        {
            SetVisible(false);
        }
    }
}
