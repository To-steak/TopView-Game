using System;

namespace TopView.Components
{
    // 몸체의 크기
    [Serializable]
    public struct BodyComponent
    {
        // 몸체 반지름(유닛, XZ). 피격 판정에 쓴다. 적은 플레이어 이동 막기에도, 플레이어는 적의 감지와 공격 범위에도 쓴다
        public float radius;
    }
}
