namespace TopView.Presentation.ViewContexts
{
    // 일시정지 연출 실행 상태. 판마다 새로 만든다.
    public class PauseViewContext
    {
        // 화면 쪽에 일시정지를 걸어 둔 상태인지. 판의 일시정지 상태가 이 값과 달라지는 프레임에만 걸거나 푼다
        public bool isApplied;
    }
}
