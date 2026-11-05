namespace TopView.Entities
{
    // 적의 행동 상태
    public enum EnemyState
    {
        // 서서 플레이어를 쫓거나 공격할 수 있는지 본다. 플레이어가 없으면 그대로 서 있다
        Idle,
        // 목적지로 걷는다. 플레이어가 있으면 목적지는 플레이어다
        Walk,
        // 공격
        Attack,
        // 사망. 다른 상태로 바뀌지 않고, 시간이 지나면 사라진다
        Die
    }
}
