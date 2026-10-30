using UnityEngine;

namespace TopView.UserInterface
{
    // 모든 UI View의 기반. Root에 붙고, Content/Header/Body/Footer 계층을 참조한다.
    public abstract class UserInterfaceView : MonoBehaviour
    {
        // View가 켜고 끄는 오브젝트. Vertical Layout Group이 붙는다.
        public RectTransform content;
        // Content의 위쪽 영역. Layout Element로 높이를 정한다.
        public RectTransform header;
        // Content의 가운데 영역. Layout Element로 높이를 정한다.
        public RectTransform body;
        // Content의 아래쪽 영역. Layout Element로 높이를 정한다.
        public RectTransform footer;
    }
}
