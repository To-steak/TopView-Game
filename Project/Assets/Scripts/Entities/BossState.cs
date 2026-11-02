namespace TopView.Entities
{
    // 보스의 행동 상태
    public enum BossState
    {
        // 등장 도발
        Taunt,
        // 플레이어 쪽으로 돌며 공격을 기다린다
        Idle,
        // 공격
        Attack,
        // 사망. 다른 상태로 바뀌지 않고, 시간이 지나면 사라진다
        Die
    }
}
