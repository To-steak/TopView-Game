namespace TopView.Components
{
    // 적 공격 종류. 판정 방식을 정한다
    public enum EnemyAttackKind
    {
        // 앞쪽 구 안의 플레이어에게 바로 피해를 준다
        Body,
        // Shots의 칸마다 그 위치에서 플레이어 쪽으로 투사체를 내보낸다
        Projectile
    }
}
