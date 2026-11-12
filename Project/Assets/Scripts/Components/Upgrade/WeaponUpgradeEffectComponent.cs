namespace TopView.Components
{
    // 런타임 값. 강화 화면에 보여 줄 무기 하나의 수치로, WeaponComponent 값의 사본이 아니라 강화 레벨을 반영해 계산한 결과다.
    // current~는 지금 레벨, next~는 한 레벨 위의 값이다. Runner가 강화 레벨로 계산해 Presenter에 넘긴다.
    // next~는 최대 레벨이어도 한 레벨 위로 계산하며, 값이 current~와 같으면 그 효과는 이 무기가 강화하지 않는다는 뜻이다.
    public struct WeaponUpgradeEffectComponent
    {
        // 지금 레벨의 피해량
        public float currentDamage;
        // 다음 레벨의 피해량
        public float nextDamage;
        // 지금 레벨의 공격 간격(틱)
        public int currentAttackIntervalTicks;
        // 다음 레벨의 공격 간격(틱)
        public int nextAttackIntervalTicks;
        // 지금 레벨의 최대 탄약 수
        public int currentMaxAmmo;
        // 다음 레벨의 최대 탄약 수
        public int nextMaxAmmo;
        // 지금 레벨의 폭발 반지름(유닛, XZ)
        public float currentExplosionRadius;
        // 다음 레벨의 폭발 반지름(유닛, XZ)
        public float nextExplosionRadius;
        // 지금 레벨의 근접 판정 반지름(유닛, XZ)
        public float currentMeleeRadius;
        // 다음 레벨의 근접 판정 반지름(유닛, XZ)
        public float nextMeleeRadius;
    }
}
