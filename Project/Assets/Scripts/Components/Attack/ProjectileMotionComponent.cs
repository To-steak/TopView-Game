using UnityEngine;

namespace TopView.Components
{
    // 런타임 값. 투사체의 이동 상태.
    public struct ProjectileMotionComponent
    {
        // 속도(월드, 유닛/초). Straight만 쓴다
        public Vector3 velocity;
        // 던진 위치(월드). Grenade만 쓴다
        public Vector3 launchPosition;
        // 착탄점(월드). Grenade만 쓴다
        public Vector3 targetPosition;
        // 생긴 뒤 지난 시간(틱)
        public int elapsedTicks;
        // 땅이나 벽에 닿았거나 착탄점에 도착해 멈췄는지. 그 틱의 판정 뒤 Straight는 사라지고, Grenade는 폭발한다
        public bool isResting;
    }
}
