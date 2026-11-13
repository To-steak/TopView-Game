using System;

namespace TopView.Components
{
    // 투사체의 폭발 범위
    [Serializable]
    public struct ExplosionComponent
    {
        // 폭발 반지름(유닛, XZ). 직선 투사체는 0보다 크면 대상이나 환경 레이어에 닿은 자리에서 폭발하고, 0이면 닿은 대상 하나만 맞힌다. 포물선 투사체는 멈춘 자리에서 늘 폭발한다
        public float radius;
    }
}
