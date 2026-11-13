using TopView.Components;
using UnityEngine;

namespace TopView.Requests
{
    // 적 스포너를 만들어 달라는 요청. 판을 준비할 때 Core가 Enemy Spawner Authoring마다 채워 큐에 넣는다.
    public struct EnemySpawnerSpawnRequest
    {
        // 위치(월드)
        public Vector3 position;
        // 회전(월드). 생기는 적이 이 방향을 본다
        public Quaternion rotation;
        // 만들 적 종류
        public EnemySpawnerComponent enemySpawner;
        // 생성 규칙
        public EnemySpawnerRuleComponent rule;
        // 감지기가 이 스포너를 가리키는 연결 번호
        public SpawnerLinkComponent link;
    }
}
