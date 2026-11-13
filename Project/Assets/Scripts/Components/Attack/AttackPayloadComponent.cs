namespace TopView.Components
{
    // 런타임 값. 투사체와 근접 판정이 맞힐 때 쓰는 진영과 피해량. 생성 때 정하며 바뀌지 않는다.
    public struct AttackPayloadComponent
    {
        // 공격한 쪽의 진영. 진영이 다른 대상만 맞힌다
        public Faction faction;
        // 맞을 때의 피해량
        public float damage;
    }
}
