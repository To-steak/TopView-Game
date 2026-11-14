namespace TopView.Components
{
    // 런타임 값. 한 판의 진행 상태. 판 시작 때 Core가 만들고 System이 바꾼다.
    public struct GameProgressComponent
    {
        // 진행 단계
        public GamePhase phase;
        // 일시정지 중인지. 진행 중(Playing)일 때만 true가 된다
        public bool isPaused;
        // 강화 화면이 열려 있는지. 열려 있는 동안 isPaused도 true다
        public bool isUpgradeOpen;
        // 남은 시간(틱)
        public int remainingTicks;
        // 점수
        public int score;
        // 이번 판에 번 골드
        public int gold;
        // 스포너에서 생기는 적의 레벨
        public int enemyLevel;
        // 다음 적 레벨 상승까지 남은 시간(틱)
        public int enemyLevelUpRemainingTicks;
    }
}
