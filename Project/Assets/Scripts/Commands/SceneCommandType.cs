namespace TopView.Commands
{
    // 씬과 앱 전환 명령의 종류
    public enum SceneCommandType
    {
        // 씬을 다시 불러 새 판을 준비한다
        Restart,
        // 게임을 끝낸다
        Exit,
        // 판을 끝내고 Menu 씬으로 돌아간다
        ReturnToMenu
    }
}
