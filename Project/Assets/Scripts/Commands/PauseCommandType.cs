namespace TopView.Commands
{
    // 일시정지 명령의 종류
    public enum PauseCommandType
    {
        // 판을 일시정지한다
        Pause,
        // 일시정지를 푼다
        Resume,
        // 강화 화면을 열고 판을 일시정지한다
        OpenUpgrade,
        // 강화 화면을 닫고 일시정지를 푼다
        CloseUpgrade
    }
}
