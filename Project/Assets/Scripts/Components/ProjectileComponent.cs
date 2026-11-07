using System;
using UnityEngine;

namespace TopView.Components
{
    // 투사체 하나의 값
    [Serializable]
    public struct ProjectileComponent
    {
        // 투사체 종류
        public ProjectileKind kind;
        // 날아가는 속도(유닛/초). Straight만 쓴다
        public float speed;
        // 피격 판정 반지름(유닛, XZ)
        public float radius;
        // 닿은 대상이 없을 때 사라지기까지의 시간(틱). Straight만 쓴다
        public int lifetimeTicks;
        // 1틱에 오르는 투척 충전량(%). 1이면 100틱(50Hz에서 2초)에 최대가 된다. 0보다 커야 한다. Grenade만 쓴다
        public float chargePerTick;
        // 충전량이 100일 때 착탄점까지의 거리(유닛, XZ). 던진 쪽 발 높이에 착탄점을 둔다. Grenade만 쓴다
        public float maxThrowDistance;
        // 던진 뒤 착탄점에 닿기까지의 시간(틱). 거리와 상관없이 같다. 0보다 커야 한다. Grenade만 쓴다
        public int flightTicks;
        // 총구와 착탄점을 이은 선분 위로 솟는 최고 높이(유닛). 비행 중간에 가장 높다. Grenade만 쓴다
        public float arcHeight;
        // 폭발 반지름(유닛, XZ). Grenade만 쓴다
        public float explosionRadius;
        // 닿으면 멈추는 환경 레이어. Straight는 닿으면 사라지고, Grenade는 그 자리에서 폭발한다
        public LayerMask collisionLayer;
        // 투사체 View 표의 번호. 판을 준비할 때 Core가 채우며, 음수면 없음
        [NonSerialized] public int viewIndex;
        // 피격 이펙트 표의 번호. 판을 준비할 때 Core가 채우며, 음수면 없음
        [NonSerialized] public int hitEffectIndex;
    }
}
