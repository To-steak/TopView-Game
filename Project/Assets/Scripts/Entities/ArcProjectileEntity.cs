using TopView.Components;

namespace TopView.Entities
{
    // 포물선 투사체 하나의 데이터
    public struct ArcProjectileEntity
    {
        // 살아 있는 칸인지. false면 빈 칸이다
        public bool isAlive;
        // 이 칸이 채워진 횟수
        public int generation;

        // 비행 값. 생성 때 무기의 값에서 복사한다
        public ArcFlightComponent flight;
        // 피격 판정 크기와 멈추는 환경 레이어
        public ProjectileHitComponent hit;
        // 폭발 범위
        public ExplosionComponent explosion;
        // 무엇으로 그릴지
        public ProjectileViewComponent projectileView;
        // 이동 상태
        public ArcProjectileMotionComponent motion;
        // 위치
        public PositionComponent position;
        // 회전
        public RotationComponent rotation;
        // 공격의 진영과 피해량
        public AttackPayloadComponent payload;
    }
}
