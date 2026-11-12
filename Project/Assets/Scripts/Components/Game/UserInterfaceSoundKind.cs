namespace TopView.Components
{
    // UI 버튼이 내는 소리 종류. Presenter가 소리 요청 목록에 넣고, UserInterfaceSoundViewSystem이 읽어 재생한다.
    public enum UserInterfaceSoundKind
    {
        // 일반 버튼을 눌렀다
        Click,
        // 종료·닫기·나가기 버튼을 눌렀다
        Close
    }
}
