namespace TopView.Components
{
    // 런타임 값. 엔티티의 마나. 최대 마나는 생성 때 정하고 강화로 오른다.
    public struct ManaComponent
    {
        // 마나
        public float current;
        // 최대 마나
        public float max;
        // 마지막으로 마나를 쓴 뒤 지난 시간(틱). 회복 지연 시간에 닿으면 더 늘리지 않는다
        public int recoveryElapsedTicks;
    }
}
