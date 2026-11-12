namespace TopView.Components
{
    // 적 공격 종류. 판정 방식과 Animator 트리거를 정한다
    public enum EnemyAttackKind
    {
        // 앞쪽 구 안의 플레이어에게 바로 피해를 준다
        Body,
        // 플레이어 쪽으로 미사일을 쏜다.
        Missile,
        // Shots의 칸마다 그 위치에서 플레이어 쪽으로 미사일을 쏜다.
        BossMissile,
        // 플레이어 쪽으로 바위를 굴린다.
        BossRock
    }
}
