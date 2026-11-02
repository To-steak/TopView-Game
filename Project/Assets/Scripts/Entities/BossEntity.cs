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

        // 레벨
        public int level;
        // 체력
        public HealthComponent health;
        // 공격력. 공격마다 배율을 곱한다
        public float attackDamage;

        // 위치
        public PositionComponent position;
        // 회전
        public RotationComponent rotation;

        // 행동 상태
        public BossState state;
        // 지금 상태에 들어간 뒤 지난 시간(틱). 들어간 틱은 0이다
        public int stateElapsedTicks;
        // 지금 공격 상태에서 쓰는 공격 번호
        public int attackIndex;
        // 지금 공격 상태에서 공격이 이미 나갔는지
        public bool isAttackCommitted;
        // BossMissile을 쏜 횟수. 짝수·홀수로 발사 위치를 번갈아 고른다
        public int missileShotCount;
    }
}
