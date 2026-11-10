namespace TopView.Commands
{
    // 판 흐름 명령의 종류
    public enum GameFlowCommandType
    {
        // 씬을 다시 불러 새 판을 준비한다
        Restart,
        // 게임을 끝낸다
        Exit,
        // 판을 일시정지한다. PauseSystem이 소비한다
        Pause,
        // 일시정지를 푼다. PauseSystem이 소비한다
        Resume,
        // 설정 화면을 연다
        OpenSettings,
        // 판을 끝내고 Menu 씬으로 돌아간다
        ReturnToMenu,
        // 강화 화면을 열고 판을 일시정지한다. PauseSystem이 소비한다
        OpenUpgrade,
        // 강화 화면을 닫고 일시정지를 푼다. PauseSystem이 소비한다
        CloseUpgrade
    }
}
