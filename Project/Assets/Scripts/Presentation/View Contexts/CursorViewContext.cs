namespace TopView.Presentation.ViewContexts
{
    // 커서 연출 실행 상태. 판마다 새로 만든다.
    public class CursorViewContext
    {
        // 크로스헤어 커서를 걸어 둔 상태인지. 걸어야 할 상태가 이 값과 달라지는 프레임에만 바꾼다
        public bool isCrosshairApplied;
    }
}
