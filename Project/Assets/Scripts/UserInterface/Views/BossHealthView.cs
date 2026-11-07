using UnityEngine;
using UnityEngine.UI;

namespace TopView.UserInterface.Views
{
    // 보스 체력 게이지를 보여 준다. 게이지는 꽉 찬 크기로 두고, 마스크가 오른쪽을 가려 비율만큼만 보인다.
    public class BossHealthView : UserInterfaceView
    {
        // 게이지 Image의 부모에 붙은 마스크. 오른쪽 여백으로 게이지를 가린다
        [SerializeField] private RectMask2D _healthMask;

        /// <summary>
        /// 게이지를 현재 체력 / 최대 체력만큼 보이게 한다.
        /// </summary>
        /// <param name="current">현재 체력</param>
        /// <param name="max">최대 체력</param>
        public void SetHealth(float current, float max)
        {
            Vector4 padding = _healthMask.padding;
            padding.z = _healthMask.rectTransform.rect.width * (1f - current / max);
            _healthMask.padding = padding;
        }
    }
}
