using TopView.Entities;

namespace TopView.Requests
{
    // 적의 행동 상태를 바꾸라는 요청. 적 System들이 넣고, 바로 뒤의 EnemyStateChangeSystem이 처리한다
    public struct EnemyStateChangeRequest
    {
        // 상태를 바꿀 적
        public EntityIdentifier target;
        // 새 상태. Attack이면 요청을 넣기 전에 공격 번호를 정해 둔다
        public EnemyState state;
    }
}
