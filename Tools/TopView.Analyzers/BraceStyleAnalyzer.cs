using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace TopView.Analyzers
{
    /// <summary>
    /// 여는 중괄호가 자기 줄에서 시작하는지(Allman) 검사한다.
    /// 블록, 타입, 네임스페이스, switch, 본문이 있는 접근자 목록은 항상 검사한다.
    /// 초기화식(<c>new X { a = 1 }</c>)과 자동 프로퍼티(<c>{ get; set; }</c>)는 여러 줄에 걸칠 때만 검사한다.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class BraceStyleAnalyzer : DiagnosticAnalyzer
    {
        private static readonly DiagnosticDescriptor _rule = new DiagnosticDescriptor(
            DIAGNOSTIC_IDENTIFIER,
            "중괄호는 다음 줄에",
            "여는 중괄호가 앞 코드와 같은 줄에 있다. 다음 줄로 내린다.",
            "Style",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public const string DIAGNOSTIC_IDENTIFIER = "TV0017";

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(_rule);

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

            context.RegisterSyntaxTreeAction(AnalyzeTree);
        }

        private static void AnalyzeTree(SyntaxTreeAnalysisContext context)
        {
            SyntaxNode root = context.Tree.GetRoot(context.CancellationToken);
            foreach (SyntaxToken token in root.DescendantTokens())
            {
                if (!token.IsKind(SyntaxKind.OpenBraceToken) || !IsChecked(token))
                {
                    continue;
                }

                SyntaxToken previous = token.GetPreviousToken();
                if (previous.IsKind(SyntaxKind.None))
                {
                    continue;
                }

                if (GetLine(previous, true) == GetLine(token, false))
                {
                    context.ReportDiagnostic(Diagnostic.Create(_rule, token.GetLocation()));
                }
            }
        }

        private static bool IsChecked(SyntaxToken openBrace)
        {
            SyntaxNode parent = openBrace.Parent;
            switch (parent)
            {
                case BlockSyntax _:
                case BaseTypeDeclarationSyntax _:
                case NamespaceDeclarationSyntax _:
                case SwitchStatementSyntax _:
                    return true;
                case AccessorListSyntax accessorList:
                    return HasAccessorBody(accessorList) || IsMultiline(parent);
                case InitializerExpressionSyntax _:
                case AnonymousObjectCreationExpressionSyntax _:
                case SwitchExpressionSyntax _:
                case PropertyPatternClauseSyntax _:
                    return IsMultiline(parent);
                default:
                    return false;
            }
        }

        private static bool HasAccessorBody(AccessorListSyntax accessorList)
        {
            foreach (AccessorDeclarationSyntax accessor in accessorList.Accessors)
            {
                if (accessor.Body != null)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsMultiline(SyntaxNode node)
        {
            FileLinePositionSpan span = node.GetLocation().GetLineSpan();
            return span.StartLinePosition.Line != span.EndLinePosition.Line;
        }

        private static int GetLine(SyntaxToken token, bool isEnd)
        {
            FileLinePositionSpan span = token.GetLocation().GetLineSpan();
            return isEnd ? span.EndLinePosition.Line : span.StartLinePosition.Line;
        }
    }
}
