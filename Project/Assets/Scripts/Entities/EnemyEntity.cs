using TopView.Components;

namespace TopView.Entities
{
    // 적 하나의 데이터
    public struct EnemyEntity
    {
        // 살아 있는 칸인지. false면 빈 칸이다
        public bool isAlive;
        // 이 칸이 채워진 횟수
        public int generation;

        // 적 종류 번호
        public EnemyKindComponent kind;
        // 처치 점수 값. 생성 때 Definition에서 복사한다
        public LevelScalingComponent score;
        // 몸체 크기. 생성 때 Definition에서 복사한다
        public BodyComponent body;
        // 이동, 회전 값. 생성 때 Definition에서 복사한다
        public NavigationMovementComponent movement;
        // 사망 상태의 길이와 사라질 때 일어나는 일. 생성 때 Definition에서 복사한다
        public DeathComponent death;
        // 공격 목록
        public AttackListComponent attackList;
        // 드롭 표
        public LootListComponent lootList;

        // 레벨과 공격력
        public CombatComponent combat;
        // 체력
        public HealthComponent health;

        // 위치(NavMesh 표면)
        public PositionComponent position;
        // 회전
        public RotationComponent rotation;

        // 행동 상태와 공격 진행
        public EnemyActionStateComponent actionState;
        // 목적지와 경로
        public NavigationAgentComponent agent;
    }
}
