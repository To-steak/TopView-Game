using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TopView.UserInterface.Views
{
    // 로딩 진행을 글자(0~100%)와 게이지로 보여 준다. 게이지는 꽉 찬 크기로 두고, 마스크가 오른쪽을 가려 비율만큼만 보인다.
    // 불러오기가 끝나면 들어가기 버튼을 보이고, 버튼 입력을 이벤트로 알린다.
    public class LoadingView : UserInterfaceView
    {
        // 들어가기 버튼을 눌렀다
        public event Action EnterClicked;

        [SerializeField] private TMP_Text _progressText;
        // 게이지 Image의 부모에 붙은 마스크. 오른쪽 여백으로 게이지를 가린다
        [SerializeField] private RectMask2D _progressMask;
        // 불러오기가 끝나면 보이는 들어가기 버튼
        [SerializeField] private Button _enterButton;

        /// <summary>
        /// 로딩 진행률을 글자와 게이지로 보여 준다.
        /// </summary>
        /// <param name="ratio">진행 비율(0~1)</param>
        public void SetProgress(float ratio)
        {
            // SetText의 서식 인자는 문자열을 새로 만들지 않아 매 프레임 불러도 할당이 없다
            _progressText.SetText("로딩 {0}%", Mathf.FloorToInt(ratio * 100f));

            Vector4 padding = _progressMask.padding;
            padding.z = _progressMask.rectTransform.rect.width * (1f - ratio);
            _progressMask.padding = padding;
        }

        /// <summary>
        /// 들어가기 버튼을 켜거나 끈다.
        /// </summary>
        /// <param name="isVisible">켜려면 true</param>
        public void SetEnterButtonVisible(bool isVisible)
        {
            _enterButton.gameObject.SetActive(isVisible);
        }

        private void OnEnable()
        {
            _enterButton.onClick.AddListener(HandleEnterButtonClicked);
        }

        private void OnDisable()
        {
            _enterButton.onClick.RemoveListener(HandleEnterButtonClicked);
        }

        private void HandleEnterButtonClicked()
        {
            EnterClicked?.Invoke();
        }
    }
}
