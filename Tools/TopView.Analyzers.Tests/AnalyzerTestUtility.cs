using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace TopView.Analyzers.Tests
{
    /// <summary>
    /// 분석기 하나를 테스트 소스에 돌려, 소스에 적어 둔 예상 경고와 실제 경고가 같은지 확인한다.
    /// 예상 경고는 그 줄 끝에 <c>// 경고: TV0002</c>처럼 적는다. 여러 개면 쉼표로 나눈다.
    /// Unity와 같게 C# 9, 문서 주석 해석 없음(DocumentationMode.None)으로 컴파일한다.
    /// </summary>
    public static class AnalyzerTestUtility
    {
        public const string PROJECT_ASSEMBLY_NAME = "TopView.Tests.Sample";

        private static readonly Regex EXPECT_PATTERN = new Regex(@"//\s*경고:\s*(?<identifiers>TV[0-9]{4}(\s*,\s*TV[0-9]{4})*)\s*$");
        private static readonly CSharpParseOptions PARSE_OPTIONS = new CSharpParseOptions(LanguageVersion.CSharp9, DocumentationMode.None);
        private static readonly Lazy<ImmutableArray<MetadataReference>> REFERENCES = new Lazy<ImmutableArray<MetadataReference>>(CreateReferences);

        /// <summary>
        /// 기본 경로(<c>Assets/Scripts/Core/Sample.cs</c>)의 소스 하나를 프로젝트 어셈블리로 컴파일해 확인한다.
        /// </summary>
        /// <param name="analyzer">시험할 분석기</param>
        /// <param name="text">예상 경고를 적은 소스</param>
        public static void Verify(DiagnosticAnalyzer analyzer, string text)
        {
            VerifyInAssembly(analyzer, PROJECT_ASSEMBLY_NAME, new TestSource(TestSource.DEFAULT_PATH, text));
        }

        /// <summary>
        /// 여러 소스를 프로젝트 어셈블리로 컴파일해 확인한다.
        /// </summary>
        /// <param name="analyzer">시험할 분석기</param>
        /// <param name="sources">예상 경고를 적은 소스들</param>
        public static void Verify(DiagnosticAnalyzer analyzer, params TestSource[] sources)
        {
            VerifyInAssembly(analyzer, PROJECT_ASSEMBLY_NAME, sources);
        }

        /// <summary>
        /// 어셈블리 이름을 정해 소스를 컴파일하고 확인한다. 프로젝트 밖 어셈블리를 시험할 때 쓴다.
        /// </summary>
        /// <param name="analyzer">시험할 분석기</param>
        /// <param name="assemblyName">컴파일할 어셈블리 이름</param>
        /// <param name="sources">예상 경고를 적은 소스들</param>
        public static void VerifyInAssembly(DiagnosticAnalyzer analyzer, string assemblyName, params TestSource[] sources)
        {
            List<SyntaxTree> trees = new List<SyntaxTree>();
            List<string> expected = new List<string>();
            foreach (TestSource source in sources)
            {
                trees.Add(CSharpSyntaxTree.ParseText(source.Text, PARSE_OPTIONS, source.Path));
                expected.AddRange(ReadExpected(source));
            }

            CSharpCompilation compilation = CSharpCompilation.Create(
                assemblyName,
                trees,
                REFERENCES.Value.Add(CreateUnityReference()),
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

            Diagnostic[] errors = compilation.GetDiagnostics().Where(IsError).ToArray();
            Assert.True(errors.Length == 0, "테스트 소스가 컴파일되지 않는다:\n" + string.Join("\n", errors.Select(FormatDiagnostic)));

            List<string> analyzerExceptions = new List<string>();
            CompilationWithAnalyzersOptions options = new CompilationWithAnalyzersOptions(
                new AnalyzerOptions(ImmutableArray<AdditionalText>.Empty),
                (exception, failedAnalyzer, diagnostic) => analyzerExceptions.Add(exception.ToString()),
                concurrentAnalysis: false,
                logAnalyzerExecutionTime: false);
            ImmutableArray<Diagnostic> diagnostics = compilation
                .WithAnalyzers(ImmutableArray.Create(analyzer), options)
                .GetAnalyzerDiagnosticsAsync()
                .GetAwaiter()
                .GetResult();

            Assert.True(analyzerExceptions.Count == 0, "분석기에서 예외가 났다:\n" + string.Join("\n", analyzerExceptions));

            string[] actual = diagnostics.Select(FormatLocation).OrderBy(SortKey).ToArray();
            Assert.Equal(expected.OrderBy(SortKey).ToArray(), actual);
        }

        private static IEnumerable<string> ReadExpected(TestSource source)
        {
            string fileName = Path.GetFileName(source.Path);
            string[] lines = source.Text.Replace("\r\n", "\n").Split('\n');
            for (int index = 0; index < lines.Length; index++)
            {
                Match match = EXPECT_PATTERN.Match(lines[index].TrimEnd());
                if (!match.Success)
                {
                    continue;
                }

                foreach (string identifier in match.Groups["identifiers"].Value.Split(','))
                {
                    yield return $"{fileName}:{index + 1}:{identifier.Trim()}";
                }
            }
        }

        private static string FormatLocation(Diagnostic diagnostic)
        {
            FileLinePositionSpan span = diagnostic.Location.GetLineSpan();
            return $"{Path.GetFileName(span.Path)}:{span.StartLinePosition.Line + 1}:{diagnostic.Id}";
        }

        private static string FormatDiagnostic(Diagnostic diagnostic)
        {
            return FormatLocation(diagnostic) + " " + diagnostic.GetMessage();
        }

        private static bool IsError(Diagnostic diagnostic)
        {
            return diagnostic.Severity == DiagnosticSeverity.Error;
        }

        // "파일:줄:규칙"을 파일, 줄 번호(숫자), 규칙 순으로 정렬하는 키. 실패 메시지에서 9번째 줄이 10번째 줄보다 앞에 오게 한다
        private static string SortKey(string value)
        {
            string[] parts = value.Split(':');
            if (parts.Length != 3 || !int.TryParse(parts[1], out int line))
            {
                return value;
            }

            return $"{parts[0]}:{line:D6}:{parts[2]}";
        }

        private static ImmutableArray<MetadataReference> CreateReferences()
        {
            string trustedAssemblies = (string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES");
            ImmutableArray<MetadataReference>.Builder references = ImmutableArray.CreateBuilder<MetadataReference>();
            foreach (string assemblyPath in trustedAssemblies.Split(Path.PathSeparator))
            {
                string assemblyName = Path.GetFileNameWithoutExtension(assemblyPath);
                if (assemblyName.StartsWith("System.", StringComparison.Ordinal) || assemblyName == "System" || assemblyName == "netstandard" || assemblyName == "mscorlib")
                {
                    references.Add(MetadataReference.CreateFromFile(assemblyPath));
                }
            }

            return references.ToImmutable();
        }

        private static MetadataReference CreateUnityReference()
        {
            CSharpCompilation unityCompilation = CSharpCompilation.Create(
                UnityStubSource.ASSEMBLY_NAME,
                new[] { CSharpSyntaxTree.ParseText(UnityStubSource.TEXT, PARSE_OPTIONS) },
                REFERENCES.Value,
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
            return unityCompilation.ToMetadataReference();
        }
    }
}
