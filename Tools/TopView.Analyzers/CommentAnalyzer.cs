using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace TopView.Analyzers
{
    /// <summary>
    /// 주석 규칙을 검사한다: public 메서드의 한국어 XML 문서 주석(TV0023), public 필드 윗줄의 한국어 한 줄 주석(TV0024).
    /// 외부 API가 정한 메서드(재정의, 인터페이스 구현)와 enum 값은 검사하지 않는다.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class CommentAnalyzer : DiagnosticAnalyzer
    {
        private static readonly DiagnosticDescriptor _methodDocumentationRule = new DiagnosticDescriptor(
            METHOD_DOCUMENTATION_DIAGNOSTIC_IDENTIFIER,
            "public 메서드 문서 주석",
            "{0}",
            "Documentation",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        private static readonly DiagnosticDescriptor _fieldCommentRule = new DiagnosticDescriptor(
            FIELD_COMMENT_DIAGNOSTIC_IDENTIFIER,
            "public 필드 주석",
            "public 필드 '{0}'의 윗줄에 한국어 주석이 없다. 선언 바로 윗줄에 한 줄 주석으로 뜻과 단위를 적는다.",
            "Documentation",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public const string METHOD_DOCUMENTATION_DIAGNOSTIC_IDENTIFIER = "TV0023";
        public const string FIELD_COMMENT_DIAGNOSTIC_IDENTIFIER = "TV0024";
        private const string SUMMARY_TAG = "summary";
        private const string RETURNS_TAG = "returns";

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(_methodDocumentationRule, _fieldCommentRule);

        /// <summary>
        /// 분석 동작을 등록한다.
        /// </summary>
        /// <param name="context">Roslyn이 넘기는 분석 등록 문맥</param>
        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterCompilationStartAction(OnCompilationStart);
        }

        private static void OnCompilationStart(CompilationStartAnalysisContext context)
        {
            if (!AnalyzerScopeUtility.IsProjectAssembly(context.Compilation))
            {
                return;
            }

            context.RegisterSyntaxNodeAction(AnalyzeMethod, SyntaxKind.MethodDeclaration);
            context.RegisterSyntaxNodeAction(AnalyzeField, SyntaxKind.FieldDeclaration);
        }

        private static void AnalyzeMethod(SyntaxNodeAnalysisContext context)
        {
            MethodDeclarationSyntax method = (MethodDeclarationSyntax)context.Node;
            if (!HasModifier(method.Modifiers, SyntaxKind.PublicKeyword) || method.Parent is InterfaceDeclarationSyntax)
            {
                return;
            }

            IMethodSymbol symbol = context.SemanticModel.GetDeclaredSymbol(method, context.CancellationToken);
            if (symbol == null || NamingUtility.IsExternallyNamed(symbol))
            {
                return;
            }

            string name = method.Identifier.ValueText;
            Location location = method.Identifier.GetLocation();
            string documentation = CommentUtility.GetDocumentationText(method);
            string summary = CommentUtility.GetTagContent(documentation, SUMMARY_TAG);
            if (string.IsNullOrEmpty(summary))
            {
                Report(context, location, $"public 메서드 '{name}'에 <summary>가 없다. 무엇을 하는지 한국어로 적는다.");
            }
            else if (!CommentUtility.ContainsHangul(summary))
            {
                Report(context, location, $"public 메서드 '{name}'의 <summary>가 한국어가 아니다. 한국어로 적는다.");
            }

            List<string> missingParameters = new List<string>();
            foreach (IParameterSymbol parameter in symbol.Parameters)
            {
                if (documentation.IndexOf("<param name=\"" + parameter.Name + "\">") < 0)
                {
                    missingParameters.Add(parameter.Name);
                }
            }

            if (missingParameters.Count > 0)
            {
                Report(context, location, $"public 메서드 '{name}'에 매개변수 {string.Join(", ", missingParameters)}의 <param>이 없다. 매개변수마다 적는다.");
            }

            if (!symbol.ReturnsVoid && CommentUtility.GetTagContent(documentation, RETURNS_TAG) == null)
            {
                Report(context, location, $"public 메서드 '{name}'에 <returns>가 없다. 돌려주는 값을 적는다.");
            }
        }

        private static void AnalyzeField(SyntaxNodeAnalysisContext context)
        {
            FieldDeclarationSyntax field = (FieldDeclarationSyntax)context.Node;
            if (!HasModifier(field.Modifiers, SyntaxKind.PublicKeyword))
            {
                return;
            }

            // 속성을 윗줄에 따로 둔 필드([Header] 등)는 속성과 선언 사이의 주석도 인정한다
            bool hasComment = HasCommentAbove(field.GetFirstToken());
            if (!hasComment && field.AttributeLists.Count > 0)
            {
                SyntaxToken attributeEnd = field.AttributeLists[field.AttributeLists.Count - 1].GetLastToken();
                SyntaxToken declarationStart = attributeEnd.GetNextToken();
                bool isOnOwnLine = GetStartLine(declarationStart.GetLocation()) > GetStartLine(attributeEnd.GetLocation());
                hasComment = isOnOwnLine && HasCommentAbove(declarationStart);
            }

            if (hasComment)
            {
                return;
            }

            SyntaxToken identifier = field.Declaration.Variables[0].Identifier;
            context.ReportDiagnostic(Diagnostic.Create(_fieldCommentRule, identifier.GetLocation(), identifier.ValueText));
        }

        private static bool HasCommentAbove(SyntaxToken token)
        {
            SyntaxTrivia? lastComment = null;
            foreach (SyntaxTrivia trivia in token.LeadingTrivia)
            {
                if (IsComment(trivia))
                {
                    lastComment = trivia;
                }
            }

            return lastComment.HasValue
                && GetEndLine(lastComment.Value.GetLocation()) == GetStartLine(token.GetLocation()) - 1
                && CommentUtility.ContainsHangul(lastComment.Value.ToString());
        }

        private static bool IsComment(SyntaxTrivia trivia)
        {
            return trivia.IsKind(SyntaxKind.SingleLineCommentTrivia)
                || trivia.IsKind(SyntaxKind.MultiLineCommentTrivia)
                || trivia.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia)
                || trivia.IsKind(SyntaxKind.MultiLineDocumentationCommentTrivia);
        }

        private static void Report(SyntaxNodeAnalysisContext context, Location location, string message)
        {
            context.ReportDiagnostic(Diagnostic.Create(_methodDocumentationRule, location, message));
        }

        private static bool HasModifier(SyntaxTokenList modifiers, SyntaxKind kind)
        {
            foreach (SyntaxToken modifier in modifiers)
            {
                if (modifier.IsKind(kind))
                {
                    return true;
                }
            }

            return false;
        }

        private static int GetStartLine(Location location)
        {
            return location.GetLineSpan().StartLinePosition.Line;
        }

        private static int GetEndLine(Location location)
        {
            // 구조화된 한 줄 문서 주석은 끝의 줄바꿈까지 포함하므로 마지막 글자의 줄을 쓴다
            FileLinePositionSpan span = location.GetLineSpan();
            return span.EndLinePosition.Character == 0 && span.EndLinePosition.Line > span.StartLinePosition.Line
                ? span.EndLinePosition.Line - 1
                : span.EndLinePosition.Line;
        }
    }
}
