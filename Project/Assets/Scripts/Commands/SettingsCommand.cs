namespace TopView.Commands
{
    // 설정 명령. 여는 명령은 MenuPresenter와 PausePresenter가, 나머지는 SettingsPresenter가 넣고, 그 씬의 흐름을 맡은 쪽(MenuFlow, Runner)이 소비한다.
    public struct SettingsCommand
    {
        // 명령 종류
        public SettingsCommandType type;
        // SetMusicVolume, SetSoundEffectVolume일 때 바꿀 볼륨(0~1). 다른 명령은 쓰지 않는다
        public float value;
    }
}
