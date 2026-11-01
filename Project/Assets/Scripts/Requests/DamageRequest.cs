using TopView.Entities;

namespace TopView.Requests
{
    // 플레이어·적·보스에게 피해를 주라는 요청. 투사체·근접 판정 System이 넣고, 다음 틱 처음에 DamageSystem이 반영한다
    public struct DamageRequest
    {
        // 피해를 받을 엔티티
        public EntityReference target;
        // 피해량
        public float amount;
    }
}
