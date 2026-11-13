namespace TopView.Commands
{
    // 씬과 앱 전환 명령. MenuPresenter, LoadingPresenter, PausePresenter, GameOverPresenter가 넣고, App이 Update에서 소비한다.
    public struct SceneCommand
    {
        // 명령 종류
        public SceneCommandType type;
    }
}
