using System;

namespace TopView.Components
{
    // 포물선 투사체를 내보내는 공격의 값. 투척 무기가 쓴다. 착탄점은 내보내는 쪽이 정한다.
    [Serializable]
    public struct ArcAttackComponent
    {
        // 내보내는 포물선 투사체
        public ArcProjectileComponent projectile;
        // 투사체를 내보내는 발마다의 시점과 위치. 칸의 startTicks 오름차순으로 둔다
        public ProjectileShotComponent[] shots;
    }
}
