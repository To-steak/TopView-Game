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
            float ratio = 0f;
            // 최대값이 0 이하면 나눌 수 없어 빈 게이지로 보인다. 값이 바뀔 때만 불리므로 경고가 매 프레임 쌓이지 않는다
            if (max <= 0f)
            {
                Debug.LogWarning($"BossHealthView >> 최대 체력이 {max}라 게이지를 비워 보인다. 최대 체력은 0보다 커야 한다.");
            }
            else
            {
                ratio = current / max;
            }

            Vector4 padding = _healthMask.padding;
            padding.z = _healthMask.rectTransform.rect.width * (1f - ratio);
            _healthMask.padding = padding;
        }
    }
}
