using TopView.Entities;

namespace TopView.Requests
{
    // 보스에게 피해를 주라는 요청. HitSystem이 넣는다
    public struct BossDamageRequest
    {
        // 피해를 받을 보스
        public EntityIdentifier target;
        // 피해량
        public float amount;
    }
}
