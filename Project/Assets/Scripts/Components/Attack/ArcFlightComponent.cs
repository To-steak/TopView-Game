using System;

namespace TopView.Components
{
    // 포물선 투사체의 비행 값
    [Serializable]
    public struct ArcFlightComponent
    {
        // 던진 뒤 착탄점에 닿기까지의 시간(틱). 거리와 상관없이 같다. 0보다 커야 한다
        public int flightTicks;
        // 던진 위치와 착탄점을 이은 선분 위로 솟는 최고 높이(유닛). 비행 중간에 가장 높다
        public float arcHeight;
    }
}
