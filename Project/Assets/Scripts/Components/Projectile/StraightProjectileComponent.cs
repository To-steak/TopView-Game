using System;

namespace TopView.Components
{
    // 직선으로 날아가 처음 닿은 대상을 맞히는 투사체의 값. 무기와 적 공격이 들고 있다가 생성 요청에 담는다
    [Serializable]
    public struct StraightProjectileComponent
    {
        // 비행 값
        public StraightFlightComponent flight;
        // 피격 판정 크기와 멈추는 환경 레이어
        public ProjectileHitComponent hit;
        // 폭발 범위. 반지름이 0이면 폭발하지 않는다
        public ExplosionComponent explosion;
    }
}
