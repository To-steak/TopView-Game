using System;
using UnityEngine;

namespace TopView.Components
{
    // 무기 하나의 값. 생성 때 Definition에서 복사하며, 게임 중의 탄약과 강화 레벨은 무기 엔티티에 있다.
    [Serializable]
    public struct WeaponComponent
    {
        // 무기 종류. 어느 공격 값을 쓰는지와 공격 상태, 탄약 규칙을 정한다
        public WeaponKind kind;
        // 한 번 맞을 때의 피해량
        public float damage;
        // 탄약 값. Ranged와 Throw가 쓴다
        public AmmoComponent ammo;
        // 공격 상태에 들어간 뒤 공격 상태가 끝나기까지의 시간(틱). Melee와 Throw가 쓰며, 공격이 모두 나가기 전에는 지나도 끝나지 않는다. Ranged는 마지막 발이 나가면 끝난다
        public int durationTicks;
        // 공격 상태에 들어간 뒤 다음 공격 상태에 들어갈 수 있기까지의 시간(틱). 공격 상태가 끝나는 시간보다 짧으면 효과가 없다. Ranged와 Melee가 쓴다
        public int attackIntervalTicks;
        // 근접 공격. Melee만 쓴다
        public MeleeAttackComponent melee;
        // 직선 투사체 공격. Ranged만 쓴다
        public StraightAttackComponent straight;
        // 포물선 투사체 공격. Throw만 쓴다
        public ArcAttackComponent arc;
        // 내보내는 투사체를 무엇으로 그릴지. Ranged와 Throw가 쓴다. 판을 준비할 때 Core가 채운다
        [NonSerialized] public ProjectileViewComponent projectileView;
        // 던지기 전에 착탄점을 정하는 값. Throw만 쓴다
        public ThrowComponent throwing;
        // 강화할 때 레벨마다 붙는 효과
        public WeaponUpgradeRuleComponent upgrade;
        // 조준 평면의 높이. 플레이어 발 높이에서 잰다(유닛). 모든 종류가 쓴다
        public float aimHeight;
        // 근접 판정을 무엇으로 그릴지. Melee만 쓴다. 판을 준비할 때 Core가 채운다
        [NonSerialized] public MeleeViewComponent meleeView;

        // 옛 칸. 값 이전 메뉴만 읽으며, 이전을 마치면 지운다
        public AttackTimingComponent timing;
        // 옛 칸. 값 이전 메뉴만 읽으며, 이전을 마치면 지운다
        public StraightProjectileComponent straightProjectile;
        // 옛 칸. 값 이전 메뉴만 읽으며, 이전을 마치면 지운다
        public ArcProjectileComponent arcProjectile;
        // 옛 칸. 값 이전 메뉴만 읽으며, 이전을 마치면 지운다
        public MeleeShapeComponent meleeShape;
        // 옛 칸. 값 이전 메뉴만 읽으며, 이전을 마치면 지운다
        public Vector3 muzzleOffset;
    }
}
