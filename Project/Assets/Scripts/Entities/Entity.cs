using TopView.Components;

namespace TopView.Entities
{
    // 엔티티 하나의 데이터. 모든 종류가 같은 칸 모양을 쓰고, mask에 켜진 컴포넌트만 뜻이 있다.
    public struct Entity
    {
        // 가진 컴포넌트. None이면 빈 칸이다
        public ComponentMask mask;
        // 이 칸이 채워진 횟수
        public int generation;

        // 위치
        public PositionComponent position;
        // 회전
        public RotationComponent rotation;

        // 전리품의 종류, 양, 남은 수명
        public LootComponent loot;

        // 근접 판정 값과 진행 시간. 판정 원의 중심은 position에 있으며 바뀌지 않는다
        public MeleeComponent melee;
        // 근접 판정이 이미 맞힌 대상. 근접 판정 구간의 칸은 기록할 배열을 저장소를 만들 때 할당해 다시 쓴다
        public MeleeHitsComponent meleeHits;
        // 공격의 진영과 피해량
        public AttackPayloadComponent payload;

        // 직선 투사체의 비행 값
        public StraightFlightComponent straightFlight;
        // 직선 투사체의 이동 상태
        public StraightProjectileMotionComponent straightMotion;
        // 포물선 투사체의 비행 값
        public ArcFlightComponent arcFlight;
        // 포물선 투사체의 이동 상태
        public ArcProjectileMotionComponent arcMotion;
        // 투사체의 피격 판정 크기와 멈추는 환경 레이어
        public ProjectileHitComponent projectileHit;
        // 폭발 범위. 반지름이 0이면 폭발하지 않는다
        public ExplosionComponent explosion;
        // 투사체를 무엇으로 그릴지
        public ProjectileViewComponent projectileView;
    }
}
