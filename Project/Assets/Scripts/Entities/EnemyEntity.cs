using TopView.Components;
using UnityEngine;

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
        public int kind;

        // 체력·공격력·점수 값. 생성 때 Definition에서 복사한다
        public EnemyStatsComponent stats;
        // 행동 값. 생성 때 Definition에서 복사한다
        public EnemyBehaviorComponent behavior;
        // 길찾기 이동 값. 생성 때 Definition에서 복사한다
        public EnemyNavigationComponent navigation;
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

        // 직전 틱의 위치(월드, NavMesh 표면). View가 보간에 쓴다
        public Vector3 previousPosition;
        // 이번 틱의 위치(월드, NavMesh 표면)
        public Vector3 currentPosition;
        // 직전 틱의 회전(월드). View가 보간에 쓴다
        public Quaternion previousRotation;
        // 이번 틱의 회전(월드)
        public Quaternion currentRotation;

        // 행동 상태
        public EnemyState state;
        // 지금 상태에 들어간 뒤 지난 시간(틱). 들어간 틱은 0이다
        public int stateElapsedTicks;
        // Idle 상태에서 마지막으로 배회 지점을 고른 뒤 지난 시간(틱)
        public int wanderElapsedTicks;
        // 지금 공격 상태에서 쓰는 공격 번호
        public int attackIndex;
        // 지금 공격 상태에서 공격이 이미 나갔는지
        public bool isAttackCommitted;

        // 목적지가 있는지
        public bool hasDestination;
        // 목적지(월드)
        public Vector3 destination;
        // 목적지에서 이 거리 안에 들면 멈춘다(유닛)
        public float stoppingDistance;
        // 목적지가 바뀌어 경로를 다시 구해야 하는지
        public bool isPathDirty;
        // 지난 이동 틱에 목적지의 멈추는 거리 안에 들었는지. 목적지를 새로 정하면 false가 된다
        public bool hasReachedDestination;
        // 경로의 꺾이는 점. 재사용하는 배열이며 pathCornerCount까지만 쓴다
        public Vector3[] pathCorners;
        // 경로의 꺾이는 점 수
        public int pathCornerCount;
        // 다음에 향할 꺾이는 점 번호
        public int pathCornerIndex;
        // 이번 틱의 속도(월드, XZ)
        public Vector3 velocity;
    }
}
