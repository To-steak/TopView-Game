namespace TopView.Editor
{
    /// <summary>
    /// 레벨 칠하기 창에서 Scene 뷰 클릭이 하는 일.
    /// </summary>
    public enum LevelPaintMode
    {
        // 격자 칸에 타일 칠하기·지우기
        Tile,
        // 격자와 상관없이 소품 놓기·지우기
        Prop,
        // 테두리와 길을 갖춘 방을 한 번에 놓기·지우기
        Room,
        // 드래그한 범위의 빈 칸을 가장 적은 수의 타일로 채우기
        Fill,
        // 두 층의 벽과 뚜껑을 갖춘 산을 한 번에 놓기·지우기
        Mountain,
    }
}
