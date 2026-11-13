using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace TopView.Analyzers
{
    /// <summary>
    /// 주석 검사에 쓰는 문서 주석 꺼내기와 한글 판정을 모은다.
    /// Unity는 문서 생성 없이 컴파일해 <c>///</c>가 일반 주석으로 읽히므로, 구조화된 문서 주석과 일반 주석을 모두 받는다.
    /// </summary>
    public static class CommentUtility
    {
        private const string DOCUMENTATION_PREFIX = "///";

        /// <summary>
        /// 선언 앞의 문서 주석(<c>///</c>) 내용을 한 문자열로 꺼낸다. <c>///</c>는 뺀다.
        /// </summary>
        /// <param name="declaration">문서 주석을 꺼낼 선언</param>
        /// <returns>문서 주석 내용. 없으면 빈 문자열</returns>
        public static string GetDocumentationText(SyntaxNode declaration)
        {
            StringBuilder builder = new StringBuilder();
            foreach (SyntaxTrivia trivia in declaration.GetLeadingTrivia())
            {
                bool isStructured = trivia.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia) || trivia.IsKind(SyntaxKind.MultiLineDocumentationCommentTrivia);
                bool isPlainDocumentation = trivia.IsKind(SyntaxKind.SingleLineCommentTrivia) && trivia.ToString().StartsWith(DOCUMENTATION_PREFIX);
                if (isStructured || isPlainDocumentation)
                {
                    builder.Append(trivia.ToFullString());
                    builder.Append('\n');
                }
            }

            return builder.Replace(DOCUMENTATION_PREFIX, string.Empty).ToString();
        }

        /// <summary>
        /// 문서 주석에서 태그 안의 내용을 꺼낸다.
        /// </summary>
        /// <param name="documentation">문서 주석 내용</param>
        /// <param name="tagName">태그 이름(예: summary)</param>
        /// <returns>태그 안의 내용. 태그가 없으면 null</returns>
        public static string GetTagContent(string documentation, string tagName)
        {
            string openTag = "<" + tagName + ">";
            string closeTag = "</" + tagName + ">";
            int openIndex = documentation.IndexOf(openTag);
            if (openIndex < 0)
            {
                return null;
            }

            int contentIndex = openIndex + openTag.Length;
            int closeIndex = documentation.IndexOf(closeTag, contentIndex);
            if (closeIndex < 0)
            {
                return null;
            }

            return documentation.Substring(contentIndex, closeIndex - contentIndex).Trim();
        }

        /// <summary>
        /// 글자 안에 한글이 있는지 확인한다.
        /// </summary>
        /// <param name="text">검사할 글자</param>
        /// <returns>한글이 하나라도 있으면 true</returns>
        public static bool ContainsHangul(string text)
        {
            if (text == null)
            {
                return false;
            }

            foreach (char character in text)
            {
                bool isSyllable = character >= '가' && character <= '힣';
                bool isJamo = (character >= 'ᄀ' && character <= 'ᇿ') || (character >= '㄰' && character <= '㆏');
                if (isSyllable || isJamo)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
