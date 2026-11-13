using System;

namespace TopView.Components
{
    // 근접 판정 구의 크기와 위치. 공격하는 쪽의 위치와 회전을 기준으로 놓는다.
    [Serializable]
    public struct MeleeShapeComponent
    {
        // 판정 구의 반지름(유닛, XZ)
        public float radius;
        // 판정 구 중심을 공격하는 쪽의 앞쪽으로 띄우는 거리(유닛)
        public float offset;
    }
}
