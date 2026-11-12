namespace TopView.Components
{
    // 런타임 값. 엔티티의 체력. 최대 체력은 생성 때 정하고, 플레이어는 강화로 오른다.
    public struct HealthComponent
    {
        // 체력
        public float current;
        // 최대 체력
        public float max;
    }
}
