using System;

namespace TopView.Components
{
    // 모든 무기에 있는 값. 판정 방식, 탄약, 공격 시간은 무기마다 가진 컴포넌트가 정한다.
    [Serializable]
    public struct WeaponComponent
    {
        // 한 번 맞을 때의 피해량
        public float damage;
        // 조준 평면의 높이. 플레이어 발 높이에서 잰다(유닛)
        public float aimHeight;

        // 옛 칸. 값 이전 메뉴만 읽으며, 이전을 마치면 지운다
        public WeaponKind kind;
        // 옛 칸. 값 이전 메뉴만 읽으며, 이전을 마치면 지운다
        public AmmoComponent ammo;
        // 옛 칸. 값 이전 메뉴만 읽으며, 이전을 마치면 지운다
        public int durationTicks;
        // 옛 칸. 값 이전 메뉴만 읽으며, 이전을 마치면 지운다
        public int attackIntervalTicks;
        // 옛 칸. 값 이전 메뉴만 읽으며, 이전을 마치면 지운다
        public MeleeAttackComponent melee;
        // 옛 칸. 값 이전 메뉴만 읽으며, 이전을 마치면 지운다
        public StraightAttackComponent straight;
        // 옛 칸. 값 이전 메뉴만 읽으며, 이전을 마치면 지운다
        public ArcAttackComponent arc;
        // 옛 칸. 값 이전 메뉴만 읽으며, 이전을 마치면 지운다
        public ThrowComponent throwing;
        // 옛 칸. 값 이전 메뉴만 읽으며, 이전을 마치면 지운다
        public WeaponUpgradeRuleComponent upgrade;
    }
}
