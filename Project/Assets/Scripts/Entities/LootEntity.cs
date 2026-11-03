using TopView.Components;

namespace TopView.Entities
{
    // 바닥에 떨어진 전리품 하나의 데이터
    public struct LootEntity
    {
        // 살아 있는 칸인지. false면 빈 칸이다
        public bool isAlive;
        // 이 칸이 채워진 횟수
        public int generation;

        // 종류, 양, 남은 수명
        public LootComponent loot;
        // 위치
        public PositionComponent position;
    }
}
