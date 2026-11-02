using TopView.Components;
using UnityEngine;

namespace TopView.Requests
{
    // 보스를 만들어 달라는 요청. 판을 준비할 때 Core가 Definition 값과 Boss Spawner Authoring의 위치를 채워 두고, WaveSystem이 레벨을 넣어 큐에 넣는다.
    public struct BossSpawnRequest
    {
        // 위치(월드)
        public Vector3 position;
        // 레벨
        public int level;
        // 체력·공격력·점수 값
        public EnemyStatsComponent stats;
        // 행동 값
        public BossBehaviorComponent behavior;
        // 공격 목록. BossSpawnSystem이 엔티티의 배열로 복사한다
        public EnemyAttackComponent[] attacks;
    }
}
