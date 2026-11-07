namespace TopView.Components
{
    // 투사체 종류. 움직임과 판정 방식을 정한다
    public enum ProjectileKind
    {
        // 직선으로 날아가 처음 닿은 대상에게 피해를 주고 사라진다. 벽은 통과한다
        Straight,
        // 총구에서 착탄점까지 포물선으로 날아가, 착탄점에 도착하거나 도중에 땅이나 벽에 닿으면 바로 폭발해 범위 안의 대상에게 피해를 준다
        Grenade
    }
}
