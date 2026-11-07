namespace TopView.Commands
{
    // 메뉴 명령의 종류
    public enum MenuCommandType
    {
        // World 씬을 비동기로 불러온다
        Start,
        // 불러오기가 끝난 World 씬으로 넘어간다
        EnterWorld,
        // 설정 화면을 연다
        OpenSettings,
        // 설정 화면을 닫고 볼륨을 저장한다
        CloseSettings,
        // 배경음악 볼륨을 바꾼다
        SetMusicVolume,
        // 효과음 볼륨을 바꾼다
        SetSoundEffectVolume,
        // 게임을 끝낸다
        Exit
    }
}
