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
        // 이번 판에 번 골드
        public int gold;
    }
}
