namespace TopView.Commands
{
    // 설정 명령의 종류. 설정 화면이 있는 씬(Menu, World)이 함께 쓴다
    public enum SettingsCommandType
    {
        // 설정 화면을 닫고 볼륨을 저장한다
        Close,
        // 배경음악 볼륨을 바꾼다
        SetMusicVolume,
        // 효과음 볼륨을 바꾼다
        SetSoundEffectVolume
    }
}
