using TopView.Components;
using UnityEngine;

namespace TopView.Requests
{
    // 보스를 만들어 달라는 요청. 지금은 이 요청을 넣는 곳이 없다.
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
