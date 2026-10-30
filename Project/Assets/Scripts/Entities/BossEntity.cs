using TopView.Components;
using UnityEngine;

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
        // 최대 체력
        public float maxHealth;
        // 체력
        public float health;
        // 공격력. 공격마다 배율을 곱한다
        public float attackDamage;

        // 직전 틱의 위치(월드). View가 보간에 쓴다
        public Vector3 previousPosition;
        // 이번 틱의 위치(월드)
        public Vector3 currentPosition;
        // 직전 틱의 회전(월드). View가 보간에 쓴다
        public Quaternion previousRotation;
        // 이번 틱의 회전(월드)
        public Quaternion currentRotation;

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
