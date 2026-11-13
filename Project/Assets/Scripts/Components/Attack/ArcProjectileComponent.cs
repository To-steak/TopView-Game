using System;

namespace TopView.Components
{
    // 착탄점까지 포물선으로 날아가 멈춘 자리에서 폭발하는 투사체의 값. 투척 무기가 들고 있다가 생성 요청에 담는다
    [Serializable]
    public struct ArcProjectileComponent
    {
        // 비행 값
        public ArcFlightComponent flight;
        // 피격 판정 크기와 멈추는 환경 레이어
        public ProjectileHitComponent hit;
        // 폭발 범위
        public ExplosionComponent explosion;
    }
}
