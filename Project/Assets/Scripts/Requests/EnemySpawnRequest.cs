using TopView.Components;
using UnityEngine;

namespace TopView.Requests
{
    // 적 하나를 만들어 달라는 요청. 판을 준비할 때 Core가 종류별로 Definition 값을 채워 두고, WaveSystem이 위치와 레벨을 넣어 큐에 넣는다.
    public struct EnemySpawnRequest
    {
        // 적 종류 번호
        public int enemyKindIndex;
        // 위치(월드). SpawnSystem이 NavMesh 위로 맞춘다
        public Vector3 position;
        // 회전(월드)
        public Quaternion rotation;
        // 레벨
        public int level;
        // 체력·공격력·점수 값
        public EnemyStatsComponent stats;
        // 행동 값
        public EnemyBehaviorComponent behavior;
        // 길찾기 이동 값
        public EnemyNavigationComponent navigation;
        // 공격 목록. SpawnSystem이 엔티티의 배열로 복사한다
        public EnemyAttackComponent[] attacks;
    }
}
