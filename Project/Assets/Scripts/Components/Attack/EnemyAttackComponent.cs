using System;

namespace TopView.Components
{
    // 적 공격 하나의 값
    [Serializable]
    public struct EnemyAttackComponent
    {
        // 공격 종류
        public EnemyAttackKind kind;
        // 이 공격을 시작할 수 있는 플레이어까지의 거리(유닛, XZ). 플레이어 반지름을 더해 비교한다
        public float attackRange;
        // 공격 상태가 끝난 뒤 이 공격을 다시 고를 수 있을 때까지의 시간(틱). 0이면 바로 고를 수 있다
        public int cooldownTicks;
        // 공격 상태의 시간 구간. startTicks와 endTicks는 Body만 쓰고, 투사체 공격의 시작은 shots의 칸마다 정한다. 공격이 모두 나가기 전에는 durationTicks가 지나도 끝나지 않는다
        public AttackTimingComponent timing;
        // 공격력에 곱하는 배율
        public float damageMultiplier;
        // Body 판정 구의 크기와 위치. Body만 쓴다
        public MeleeShapeComponent meleeShape;
        // 내보내는 투사체. Body가 아닌 공격이 쓴다
        public ProjectileComponent projectile;
        // 내보내는 투사체를 무엇으로 그릴지. Body가 아닌 공격이 쓴다. 판을 준비할 때 Core가 채운다
        [NonSerialized] public ProjectileViewComponent projectileView;
        // 투사체를 내보내는 발마다의 시점과 위치. 칸의 startTicks 오름차순으로 둔다. Body가 아닌 공격이 쓴다
        public ProjectileShotComponent[] shots;
    }
}
