namespace TopView.Editor
{
    /// <summary>
    /// 빈 공간 안에서 안쪽으로 꺾인 모서리 둘을 잇는 직선. 에디터 전용.
    /// 가로 선이면 line이 격자선의 Z 번호이고 start, end가 X 번호다. 세로 선이면 반대다.
    /// </summary>
    public struct LevelFillChord
    {
        // 선이 놓인 격자선 번호
        public int line;
        // 선의 시작 번호
        public int start;
        // 선의 끝 번호 (start보다 크다)
        public int end;
    }
}
