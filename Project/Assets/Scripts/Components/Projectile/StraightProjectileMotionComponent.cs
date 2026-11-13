using UnityEngine;

namespace TopView.Components
{
    // 런타임 값. 직선 투사체의 이동 상태.
    public struct StraightProjectileMotionComponent
    {
        // 속도(월드, 유닛/초)
        public Vector3 velocity;
        // 생긴 뒤 지난 시간(틱)
        public int elapsedTicks;
        // 환경에 닿아 멈췄는지. 그 틱의 판정 뒤 사라진다
        public bool isResting;
    }
}
