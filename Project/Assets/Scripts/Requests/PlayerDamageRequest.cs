using TopView.Entities;

namespace TopView.Requests
{
    // 플레이어에게 피해를 주라는 요청
    public struct PlayerDamageRequest
    {
        // 피해를 받을 플레이어
        public EntityIdentifier target;
        // 피해량
        public float amount;
    }
}
