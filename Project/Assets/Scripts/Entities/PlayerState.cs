namespace TopView.Entities
{
    // 플레이어의 행동 상태
    public enum PlayerState
    {
        // 서 있음
        Idle,
        // 걷거나 달림
        Move,
        // 점프 중
        Jump,
        // 착지 동작
        Land,
        // 회피
        Dodge,
        // 원거리 무기 공격
        Shot,
        // 근접 무기 공격
        Swing,
        // 투척 무기 공격
        Throw,
        // 무기 교체
        Swap,
        // 재장전
        Reload,
        // 사망. 다른 상태로 바뀌지 않는다
        Die
    }
}
