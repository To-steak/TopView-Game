using System;

namespace TopView.Components
{
    // 적 공격 하나의 값
    [Serializable]
    public struct EnemyAttackComponent
    {
        // 공격 종류. 어느 공격 값을 쓰는지 정한다
        public EnemyAttackKind kind;
        // 이 공격을 시작할 수 있는 플레이어까지의 거리(유닛, XZ). 플레이어 반지름을 더해 비교한다
        public float attackRange;
        // 공격 상태가 끝난 뒤 이 공격을 다시 고를 수 있을 때까지의 시간(틱). 0이면 바로 고를 수 있다
        public int cooldownTicks;
        // 공격 상태에 들어간 뒤 공격 상태가 끝나기까지의 시간(틱). 공격이 모두 나가기 전에는 지나도 끝나지 않는다
        public int durationTicks;
        // 공격력에 곱하는 배율
        public float damageMultiplier;
        // 근접 공격. Body만 쓴다
        public MeleeAttackComponent melee;
        // 직선 투사체 공격. Projectile만 쓴다
        public StraightAttackComponent straight;
        // 내보내는 투사체를 무엇으로 그릴지. Projectile만 쓴다. 판을 준비할 때 Core가 채운다
        [NonSerialized] public ProjectileViewComponent projectileView;

        // 옛 칸. 값 이전 메뉴만 읽으며, 이전을 마치면 지운다
        public AttackTimingComponent timing;
        // 옛 칸. 값 이전 메뉴만 읽으며, 이전을 마치면 지운다
        public MeleeShapeComponent meleeShape;
        // 옛 칸. 값 이전 메뉴만 읽으며, 이전을 마치면 지운다
        public StraightProjectileComponent straightProjectile;
        // 옛 칸. 값 이전 메뉴만 읽으며, 이전을 마치면 지운다
        public ProjectileShotComponent[] shots;
    }
}
