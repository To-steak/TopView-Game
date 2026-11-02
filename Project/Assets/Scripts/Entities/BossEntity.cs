using TopView.Components;

namespace TopView.Entities
{
    // 보스 하나의 데이터
    public struct BossEntity
    {
        // 살아 있는 칸인지. false면 빈 칸이다
        public bool isAlive;
        // 이 칸이 채워진 횟수
        public int generation;

        // 체력·공격력·점수 값. 생성 때 Definition에서 복사한다
        public EnemyStatsComponent stats;
        // 행동 값. 생성 때 Definition에서 복사한다
        public BossBehaviorComponent behavior;
        // 공격 목록. 생성 때 Definition에서 복사한다
        public EnemyAttackComponent[] attacks;

        // 레벨과 공격력
        public CombatComponent combat;
        // 체력
        public HealthComponent health;

        // 위치
        public PositionComponent position;
        // 회전
        public RotationComponent rotation;

        // 행동 상태와 공격 진행
        public BossActionStateComponent actionState;
    }
}
