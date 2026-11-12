namespace TopView.Components
{
    // 바닥에 떨어진 전리품의 런타임 값
    public struct LootComponent
    {
        // 전리품 종류
        public LootKind kind;
        // 획득할 때 얻는 양
        public int amount;
        // 사라지기까지 남은 시간(틱)
        public int remainingTicks;
    }
}
