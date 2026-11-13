namespace TopView.Components
{
    // 적 공격 종류. 어느 공격 값을 쓰는지 정한다
    public enum EnemyAttackKind
    {
        // melee 값으로 앞쪽 구 안의 플레이어에게 피해를 준다
        Body,
        // straight 값으로 발마다 그 위치에서 플레이어 쪽으로 직선 투사체를 내보낸다
        Projectile
    }
}
