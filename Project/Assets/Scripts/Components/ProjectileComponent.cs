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
        // 날아가는 속도(유닛/초)
        public float speed;
        // 피격 판정 반지름(유닛, XZ)
        public float radius;
        // 닿은 대상이 없을 때 사라지기까지의 시간(틱). Straight만 쓴다
        public int lifetimeTicks;
        // 던질 때 앞 방향에 더하는 위쪽 비율. Grenade만 쓴다
        public float upwardRatio;
        // 중력 가속도(유닛/초²). 아래쪽이 음수. Grenade만 쓴다
        public float gravity;
        // 던진 뒤 폭발하기까지의 시간(틱). Grenade만 쓴다
        public int fuseTicks;
        // 폭발 반지름(유닛, XZ). Grenade만 쓴다
        public float explosionRadius;
        // 닿으면 멈추는 환경 레이어. Grenade만 쓴다
        public LayerMask collisionLayer;
        // 투사체 View 표의 번호. 판을 준비할 때 Core가 채우며, 음수면 없음
        [NonSerialized] public int viewIndex;
        // 피격 이펙트 표의 번호. 판을 준비할 때 Core가 채우며, 음수면 없음
        [NonSerialized] public int hitEffectIndex;
    }
}
