using TopView.Components;

namespace TopView.Entities
{
    // 투사체 하나의 데이터
    public struct ProjectileEntity
    {
        // 살아 있는 칸인지. false면 빈 칸이다
        public bool isAlive;
        // 이 칸이 채워진 횟수
        public int generation;

        // 투사체 값. 생성 때 무기나 공격의 값에서 복사한다
        public ProjectileComponent projectile;
        // 이동 상태
        public ProjectileMotionComponent motion;
        // 위치
        public PositionComponent position;
        // 회전
        public RotationComponent rotation;
        // 공격의 진영과 피해량
        public AttackPayloadComponent payload;
    }
}
