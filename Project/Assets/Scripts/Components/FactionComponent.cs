namespace TopView.Components
{
    // 런타임 값. 공격을 맞을 수 있는 엔티티의 진영. 생성 때 정하며 바뀌지 않는다.
    public struct FactionComponent
    {
        // 진영. 공격한 쪽과 진영이 다르면 맞는다
        public Faction faction;
    }
}
