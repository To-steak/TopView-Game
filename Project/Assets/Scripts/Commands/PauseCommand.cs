namespace TopView.Commands
{
    // 일시정지 명령. PausePresenter, UpgradePresenter와 Runner의 ESC·강화 입력이 넣고, PauseSystem이 소비한다.
    public struct PauseCommand
    {
        // 명령 종류
        public PauseCommandType type;
    }
}
