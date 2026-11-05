using System;

namespace TopView.Components
{
    // 적 스포너의 생성 규칙 설정값. Enemy Spawner Authoring에서 정한다.
    [Serializable]
    public struct EnemySpawnerRuleComponent
    {
        // 다음 적을 내보내기까지의 간격(틱). 1이면 매 틱 내보낸다
        public int spawnIntervalTicks;
        // 이 스포너가 한 판에 만드는 적의 누적 최대 마릿수
        public int maxSpawnCount;
    }
}
