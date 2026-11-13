using TopView.Components;
using UnityEngine;

namespace TopView.Requests
{
    // 적 하나를 만들어 달라는 요청. 판을 준비할 때 Core가 종류별로 Definition 값을 채워 두고, EnemySpawnerSystem이 위치와 레벨을 넣어 큐에 넣는다.
    public struct EnemySpawnRequest
    {
        // 적 종류 번호
        public int enemyKindIndex;
        // 위치(월드). EnemySpawnSystem이 NavMesh 위로 맞춘다
        public Vector3 position;
        // 회전(월드)
        public Quaternion rotation;
        // 레벨
        public int level;
        // 최대 체력
        public LevelScalingComponent maxHealth;
        // 공격력
        public LevelScalingComponent attackDamage;
        // 최고 이동 속도(유닛/초)
        public LevelScalingComponent speed;
        // 처치 점수
        public LevelScalingComponent score;
        // 몸체 크기
        public BodyComponent body;
        // 이동, 회전 값
        public NavigationMovementComponent movement;
        // 사망 상태의 길이와 사라질 때 일어나는 일
        public DeathComponent death;
        // 공격 목록. EnemySpawnSystem이 엔티티의 배열로 복사한다
        public EnemyAttackComponent[] attacks;
        // 드롭 표. EnemySpawnSystem이 엔티티의 배열로 복사한다
        public EnemyLootComponent[] loots;
    }
}
