namespace TopView.Components
{
    // 런타임 값. 적의 레벨과 그에 맞춰 생성 때 계산한 공격력.
    public struct CombatComponent
    {
        // 레벨
        public int level;
        // 공격력. 공격마다 배율을 곱한다
        public float attackDamage;
    }
}
