namespace TopView.Components
{
    // 한 판의 진행 상태. 판 시작 때 Core가 만들고 System이 바꾼다.
    public struct GameProgressComponent
    {
        // 진행 단계
        public GamePhase phase;
        // 남은 시간(틱)
        public int remainingTicks;
        // 점수
        public int score;
        // 웨이브별 시작 여부. 길이는 웨이브 수
        public bool[] isWaveStarted;
        // 적 종류별 스폰 여부. 길이는 적 종류 수
        public bool[] isEnemyKindSpawning;
        // 적 종류별 다음 스폰까지 남은 시간(틱). 길이는 적 종류 수
        public int[] enemyKindSpawnRemainingTicks;
    }
}
