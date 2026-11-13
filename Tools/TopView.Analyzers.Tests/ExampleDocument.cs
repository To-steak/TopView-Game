using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace TopView.Analyzers.Tests
{
    /// <summary>
    /// 규칙 예시 문서(<c>Examples/*.md</c>)를 읽어 검사할 절로 나눈다.
    /// 문서 형식:
    /// <list type="bullet">
    /// <item>첫 줄 근처의 <c>분석기: 클래스이름</c> 줄로 검사할 분석기를 정한다.</item>
    /// <item><c>## 제목</c>마다 검사 하나다. 그 아래 <c>```csharp</c> 블록들을 한 번에 컴파일한다.</item>
    /// <item>블록 여는 줄에 <c>```csharp path=경로</c>를 적으면 그 경로의 파일로 컴파일한다. 없으면 <c>Assets/Scripts/Core/Sample.cs</c>다.</item>
    /// <item>블록 여는 줄에 <c>```csharp assembly=이름</c>을 적으면 그 어셈블리 이름으로 컴파일한다.</item>
    /// <item>경고가 나와야 하는 줄 끝에 <c>// 경고: TV0006</c>을 적는다.</item>
    /// </list>
    /// </summary>
    public static class ExampleDocument
    {
        public const string EXAMPLES_FOLDER = "Examples";
        public const string README_FILE_NAME = "README.md";

        private const string ANALYZER_PREFIX = "분석기:";
        private const string SECTION_PREFIX = "## ";
        private const string CODE_OPEN = "```csharp";
        private const string CODE_CLOSE = "```";
        private const string PATH_KEY = "path=";
        private const string ASSEMBLY_KEY = "assembly=";

        /// <summary>
        /// 예시 문서가 모인 폴더의 경로를 돌려준다. 빌드가 문서를 출력 폴더로 복사한다.
        /// </summary>
        /// <returns>예시 폴더의 절대 경로</returns>
        public static string GetExamplesDirectory()
        {
            return Path.Combine(AppContext.BaseDirectory, EXAMPLES_FOLDER);
        }

        /// <summary>
        /// 검사할 예시 문서의 파일 이름을 이름순으로 돌려준다. README는 뺀다.
        /// </summary>
        /// <returns>문서 파일 이름 목록</returns>
        public static List<string> GetDocumentFileNames()
        {
            List<string> fileNames = new List<string>();
            foreach (string documentPath in Directory.GetFiles(GetExamplesDirectory(), "*.md"))
            {
                string fileName = Path.GetFileName(documentPath);
                if (fileName != README_FILE_NAME)
                {
                    fileNames.Add(fileName);
                }
            }

            fileNames.Sort(StringComparer.Ordinal);
            return fileNames;
        }

        /// <summary>
        /// 예시 문서 하나를 읽어 절 목록으로 나눈다.
        /// </summary>
        /// <param name="fileName">예시 폴더 안의 문서 파일 이름</param>
        /// <returns>문서의 절 목록</returns>
        public static List<ExampleSection> Load(string fileName)
        {
            string[] lines = File.ReadAllLines(Path.Combine(GetExamplesDirectory(), fileName));
            string analyzerName = FindAnalyzerName(lines, fileName);

            List<ExampleSection> sections = new List<ExampleSection>();
            ExampleSection currentSection = null;
            StringBuilder code = null;
            string codePath = null;
            for (int index = 0; index < lines.Length; index++)
            {
                string line = lines[index];
                if (code != null)
                {
                    if (line.Trim() == CODE_CLOSE)
                    {
                        currentSection.Sources.Add(new TestSource(codePath, code.ToString()));
                        code = null;
                        continue;
                    }

                    code.Append(line).Append('\n');
                    continue;
                }

                if (line.StartsWith(SECTION_PREFIX, StringComparison.Ordinal))
                {
                    currentSection = new ExampleSection(line.Substring(SECTION_PREFIX.Length).Trim(), analyzerName);
                    sections.Add(currentSection);
                    continue;
                }

                if (!line.TrimStart().StartsWith(CODE_OPEN, StringComparison.Ordinal))
                {
                    continue;
                }

                if (currentSection == null)
                {
                    throw new InvalidDataException($"{fileName} {index + 1}번째 줄: 코드 블록이 ## 절 밖에 있다.");
                }

                string info = line.TrimStart().Substring(CODE_OPEN.Length).Trim();
                codePath = TestSource.DEFAULT_PATH;
                if (info.StartsWith(PATH_KEY, StringComparison.Ordinal))
                {
                    codePath = info.Substring(PATH_KEY.Length).Trim();
                }
                else if (info.StartsWith(ASSEMBLY_KEY, StringComparison.Ordinal))
                {
                    currentSection.AssemblyName = info.Substring(ASSEMBLY_KEY.Length).Trim();
                }

                code = new StringBuilder();
            }

            if (code != null)
            {
                throw new InvalidDataException($"{fileName}: 닫히지 않은 코드 블록이 있다.");
            }

            return sections;
        }

        private static string FindAnalyzerName(string[] lines, string fileName)
        {
            foreach (string line in lines)
            {
                string trimmed = line.Trim().TrimStart('>', '*', ' ');
                if (trimmed.StartsWith(ANALYZER_PREFIX, StringComparison.Ordinal))
                {
                    return trimmed.Substring(ANALYZER_PREFIX.Length).Trim().Trim('`', '*', ' ');
                }
            }

            throw new InvalidDataException($"{fileName}: '{ANALYZER_PREFIX} 클래스이름' 줄이 없다.");
        }
    }
}
