using UnityEngine;

namespace TopView.Components
{
    // 런타임 값. 투사체의 이동 상태.
    public struct ProjectileMotionComponent
    {
        // 속도(월드, 유닛/초)
        public Vector3 velocity;
        // 생긴 뒤 지난 시간(틱)
        public int elapsedTicks;
        // 땅이나 벽에 닿아 멈췄는지. Grenade는 그 자리에서 폭발을 기다리고, Straight는 그 틱의 명중 판정 뒤 사라진다
        public bool isResting;
    }
}
