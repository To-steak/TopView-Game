using TopView.Entities;

namespace TopView.Components
{
    // 런타임 값. 근접 판정 하나의 값과 진행 시간. 판정 원의 중심은 PositionComponent에 있으며 바뀌지 않는다.
    public struct MeleeComponent
    {
        // 공격한 엔티티
        public EntityIdentifier owner;
        // 판정 원의 반지름(유닛, XZ)
        public float radius;
        // 판정하는 시간(틱). 1 이하이면 한 틱만 판정한다
        public int durationTicks;
        // 판정을 시작한 뒤 지난 시간(틱)
        public int elapsedTicks;
    }
}
