namespace TopView.Components
{
    // 런타임 값. 엔티티의 마나.
    public struct ManaComponent
    {
        // 마나
        public float current;
        // 마지막으로 마나를 쓴 뒤 지난 시간(틱). 회복 지연 시간에 닿으면 더 늘리지 않는다
        public int recoveryElapsedTicks;
    }
}
