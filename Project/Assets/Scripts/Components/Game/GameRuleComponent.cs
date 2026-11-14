using System;

namespace TopView.Components
{
    // 한 판의 규칙 값. 판 시작 때 Definition에서 World로 복사한다.
    [Serializable]
    public struct GameRuleComponent
    {
        // 제한 시간(틱). 0이 되면 게임오버
        public int timeLimitTicks;
        // 판 시작 때 스포너에서 생기는 적의 레벨
        public int startingEnemyLevel;
        // 적 레벨이 1 오르는 간격(틱)
        public int enemyLevelUpIntervalTicks;
        // World의 난수 생성기 시드
        public int randomSeed;
        // 적의 목적지와 생성 위치를 NavMesh 위로 맞출 때 찾는 최대 거리(유닛)
        public float navMeshSampleDistance;
        // 경로를 구하지 못한 적이 다시 길을 찾기까지 기다리는 시간(틱). 그동안 Idle로 서서 플레이어 쪽을 바라본다. 0 이하면 기다리지 않는다
        public int navigationRetryIntervalTicks;
        // 탈출할 때 NavMesh를 찾는 가장 낮은 높이(월드 Y). 플레이어가 이보다 아래로 빠졌으면 같은 XZ의 이 높이에서 찾는다
        public float unstuckSampleMinimumHeight;
        // 탈출할 때 NavMesh를 찾는 최대 거리(유닛)
        public float unstuckSampleDistance;
    }
}
