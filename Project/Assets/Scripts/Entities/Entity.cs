using TopView.Components;

namespace TopView.Entities
{
    // 엔티티 하나의 데이터. 모든 종류가 같은 칸 모양을 쓰고, mask에 켜진 컴포넌트만 뜻이 있다.
    public struct Entity
    {
        // 가진 컴포넌트. None이면 빈 칸이다
        public ComponentMask mask;
        // 이 칸이 채워진 횟수
        public int generation;

        // 위치
        public PositionComponent position;
        // 전리품의 종류, 양, 남은 수명
        public LootComponent loot;
    }
}
