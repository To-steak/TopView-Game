using System.Collections.Generic;

namespace TopView.Analyzers.Tests
{
    /// <summary>
    /// 예시 문서의 <c>##</c> 절 하나. 절 안의 코드 블록을 한 번에 컴파일해 분석기 하나로 검사한다.
    /// </summary>
    public sealed class ExampleSection
    {
        /// <summary>
        /// 절을 만든다.
        /// </summary>
        /// <param name="title">절 제목</param>
        /// <param name="analyzerName">문서 머리에 적은 분석기 클래스 이름</param>
        public ExampleSection(string title, string analyzerName)
        {
            Title = title;
            AnalyzerName = analyzerName;
            AssemblyName = AnalyzerTestUtility.PROJECT_ASSEMBLY_NAME;
            Sources = new List<TestSource>();
        }

        public string Title { get; }

        public string AnalyzerName { get; }

        public string AssemblyName { get; set; }

        public List<TestSource> Sources { get; }
    }
}
