using UnityEngine;

namespace TopView.Components
{
    // 적 스포너. 판 시작 때 Core가 Enemy Spawner Authoring에서 채우고, 게임 중에는 바꾸지 않는다.
    public struct EnemySpawnerComponent
    {
        // 이 스포너에서 생기는 적 종류 번호
        public int enemyKindIndex;
        // 위치(월드)
        public Vector3 position;
        // 회전(월드)
        public Quaternion rotation;
        // 생성 규칙 설정값
        public EnemySpawnerRuleComponent rule;
    }
}
