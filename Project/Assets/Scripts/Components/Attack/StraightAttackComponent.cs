using System;

namespace TopView.Components
{
    // 직선 투사체를 내보내는 공격의 값. 원거리 무기와 적의 투사체 공격이 쓴다.
    [Serializable]
    public struct StraightAttackComponent
    {
        // 내보내는 직선 투사체
        public StraightProjectileComponent projectile;
        // 투사체를 내보내는 발마다의 시점과 위치. 칸의 startTicks 오름차순으로 둔다
        public ProjectileShotComponent[] shots;
    }
}
