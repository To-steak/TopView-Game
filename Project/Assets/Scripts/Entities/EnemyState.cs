namespace TopView.Entities
{
    // 적의 행동 상태
    public enum EnemyState
    {
        // 서서 플레이어를 찾고, 일정 시간마다 배회 지점을 고른다
        Idle,
        // 목적지(플레이어나 배회 지점)로 걷는다
        Walk,
        // 공격
        Attack,
        // 사망. 다른 상태로 바뀌지 않고, 시간이 지나면 사라진다
        Die
    }
}
