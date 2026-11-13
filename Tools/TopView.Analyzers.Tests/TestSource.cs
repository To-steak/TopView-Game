namespace TopView.Analyzers.Tests
{
    /// <summary>
    /// 테스트에서 컴파일할 소스 파일 하나. 경로는 Unity가 넘기는 형식(<c>Assets/Scripts/...</c>)으로 적는다.
    /// </summary>
    public sealed class TestSource
    {
        public const string DEFAULT_PATH = "Assets/Scripts/Core/Sample.cs";

        /// <summary>
        /// 소스 파일을 만든다.
        /// </summary>
        /// <param name="path">컴파일러에 넘길 파일 경로</param>
        /// <param name="text">소스 코드</param>
        public TestSource(string path, string text)
        {
            Path = path;
            Text = text;
        }

        public string Path { get; }

        public string Text { get; }
    }
}
