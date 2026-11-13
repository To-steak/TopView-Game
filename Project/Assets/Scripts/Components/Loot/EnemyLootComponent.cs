using System;

namespace TopView.Components
{
    // 적 드롭 표의 항목 하나. 양은 기본값 + 레벨 × 증가량이다
    [Serializable]
    public struct EnemyLootComponent
    {
        // 전리품 종류. None이면 아무것도 떨어뜨리지 않는다
        public LootKind kind;
        // 뽑힐 비율. 표 전체 가중치 합에 대한 비율로 뽑는다
        public int weight;
        // 레벨 0일 때의 양. None은 쓰지 않는다
        public int baseAmount;
        // 레벨 1마다 양에 더하는 값. None은 쓰지 않는다
        public int amountIncrement;
    }
}
