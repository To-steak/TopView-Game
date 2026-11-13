using System;

namespace TopView.Components
{
    // 직선 투사체의 비행 값
    [Serializable]
    public struct StraightFlightComponent
    {
        // 날아가는 속도(유닛/초)
        public float speed;
        // 닿은 대상이 없을 때 사라지기까지의 시간(틱)
        public int lifetimeTicks;
    }
}
