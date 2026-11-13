using System;
using UnityEngine;

namespace TopView.Components
{
    // 투사체의 피격 판정 크기와 멈추는 환경 레이어
    [Serializable]
    public struct ProjectileHitComponent
    {
        // 피격 판정 반지름(유닛, XZ). 환경에 닿는지 검사하는 구의 반지름으로도 쓴다
        public float radius;
        // 닿으면 멈추는 환경 레이어. 직선 투사체는 닿으면 사라지고, 포물선 투사체는 그 자리에서 폭발한다
        public LayerMask collisionLayer;
    }
}
