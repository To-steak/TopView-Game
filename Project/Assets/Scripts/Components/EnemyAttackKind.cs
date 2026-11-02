namespace TopView.Components
{
    // 적·보스 공격 종류. 판정 방식과 Animator 트리거를 정한다
    public enum EnemyAttackKind
    {
        // 앞쪽 구 안의 플레이어에게 바로 피해를 준다
        Body,
        // 앞쪽으로 미사일 하나를 쏜다.
        Missile,
        // 좌우를 번갈아 미사일 하나를 쏜다.
        BossMissile,
        // 앞쪽으로 바위를 굴린다.
        BossRock
    }
}
