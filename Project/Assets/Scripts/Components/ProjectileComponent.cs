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
        // 충전량이 100일 때 던지는 거리(유닛, XZ). 던진 높이와 같은 높이에 떨어진다고 보고 속도를 구한다. Grenade만 쓴다
        public float maxThrowDistance;
        // 던질 때 앞 방향에 더하는 위쪽 비율. 던지는 각도를 정하며 0보다 커야 한다. Grenade만 쓴다
        public float upwardRatio;
        // 중력 가속도(유닛/초²). 아래쪽이 음수. Grenade만 쓴다
        public float gravity;
        // 던진 뒤 폭발하기까지의 시간(틱). Grenade만 쓴다
        public int fuseTicks;
        // 폭발 반지름(유닛, XZ). Grenade만 쓴다
        public float explosionRadius;
        // 닿으면 멈추는 환경 레이어. Straight는 닿으면 사라지고, Grenade는 그 자리에 멈춘다
        public LayerMask collisionLayer;
        // 투사체 View 표의 번호. 판을 준비할 때 Core가 채우며, 음수면 없음
        [NonSerialized] public int viewIndex;
        // 피격 이펙트 표의 번호. 판을 준비할 때 Core가 채우며, 음수면 없음
        [NonSerialized] public int hitEffectIndex;
    }
}
