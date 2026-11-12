using System;

namespace TopView.Components
{
    // 몸체의 크기
    [Serializable]
    public struct BodyComponent
    {
        // 몸체 반지름(유닛, XZ). 피격 판정과 플레이어 이동 막기에 쓴다
        public float radius;
    }
}
