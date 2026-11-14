using TopView.Entities;

namespace TopView.Requests
{
    // 적을 없애라는 요청. EnemyDeathEndSystem이 넣고, 바로 뒤의 EnemyDespawnSystem이 처리한다
    public struct EnemyDespawnRequest
    {
        // 없앨 적
        public EntityIdentifier target;
    }
}
