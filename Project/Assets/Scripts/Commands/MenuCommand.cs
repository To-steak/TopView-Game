namespace TopView.Commands
{
    // 메뉴 명령. 메뉴 씬의 UI Presenter가 넣고 MenuFlow가 소비한다.
    public struct MenuCommand
    {
        // 명령 종류
        public MenuCommandType type;
        // SetMusicVolume, SetSoundEffectVolume일 때 바꿀 볼륨(0~1). 다른 명령은 쓰지 않는다
        public float value;
    }
}
