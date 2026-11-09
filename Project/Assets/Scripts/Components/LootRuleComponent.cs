using System;

namespace TopView.Components
{
    // 전리품 규칙 값. 판 시작 때 Definition에서 World로 복사한다.
    [Serializable]
    public struct LootRuleComponent
    {
        // 플레이어 중심에서 전리품을 획득하는 반경(유닛, XZ)
        public float pickupRadius;
        // 전리품이 생긴 뒤 사라지기까지의 시간(틱)
        public int lifetimeTicks;
    }
}
