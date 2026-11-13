using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace TopView.Analyzers.Tests
{
    /// <summary>
    /// 예시 문서의 절마다, 분석기가 문서에 적힌 대로 경고하는지 검사한다.
    /// </summary>
    public sealed class ExampleTests
    {
        /// <summary>
        /// 예시 폴더의 모든 문서와 절을 검사 목록으로 돌려준다. xUnit이 절마다 테스트 하나로 보여 준다.
        /// </summary>
        /// <returns>(문서 파일 이름, 절 제목) 목록</returns>
        public static IEnumerable<object[]> GetSections()
        {
            foreach (string fileName in ExampleDocument.GetDocumentFileNames())
            {
                foreach (ExampleSection section in ExampleDocument.Load(fileName))
                {
                    yield return new object[] { fileName, section.Title };
                }
            }
        }

        [Theory]
        [MemberData(nameof(GetSections))]
        public void AnalyzerFollowsExample(string fileName, string sectionTitle)
        {
            ExampleSection section = ExampleDocument.Load(fileName).Find(section => section.Title == sectionTitle);
            Assert.NotNull(section);
            Assert.True(section.Sources.Count > 0, $"{fileName}의 '{sectionTitle}' 절에 코드 블록이 없다.");

            AnalyzerTestUtility.VerifyInAssembly(CreateAnalyzer(section.AnalyzerName), section.AssemblyName, section.Sources.ToArray());
        }

        /// <summary>
        /// 이름으로 분석기 인스턴스를 만든다.
        /// </summary>
        /// <param name="analyzerName">TopView.Analyzers 네임스페이스의 분석기 클래스 이름</param>
        /// <returns>분석기 인스턴스</returns>
        public static DiagnosticAnalyzer CreateAnalyzer(string analyzerName)
        {
            Type analyzerType = typeof(AnalyzerScopeUtility).Assembly.GetType("TopView.Analyzers." + analyzerName);
            Assert.True(analyzerType != null, $"분석기 '{analyzerName}'를 찾지 못했다. 문서의 '분석기:' 줄을 확인한다.");
            return (DiagnosticAnalyzer)Activator.CreateInstance(analyzerType);
        }
    }
}
