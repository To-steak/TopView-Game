using UnityEngine;

namespace TopView.Components
{
    // 런타임 값. 엔티티의 회전. 매 틱 System이 바꾸고 View가 보간에 쓴다.
    public struct RotationComponent
    {
        // 직전 틱의 회전(월드)
        public Quaternion previous;
        // 이번 틱의 회전(월드)
        public Quaternion current;
    }
}
