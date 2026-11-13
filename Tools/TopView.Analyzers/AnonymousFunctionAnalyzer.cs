using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace TopView.Analyzers
{
    /// <summary>
    /// 람다식과 익명 메서드를 찾는다. 식 본문 멤버(<c>public int Count => …;</c>)는 익명 함수가 아니므로 검사하지 않는다.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class AnonymousFunctionAnalyzer : DiagnosticAnalyzer
    {
        private static readonly DiagnosticDescriptor _rule = new DiagnosticDescriptor(
            DIAGNOSTIC_IDENTIFIER,
            "익명 함수 금지",
            "익명 함수를 쓰지 않는다. 메서드를 만들고 이름을 넘긴다.",
            "Style",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public const string DIAGNOSTIC_IDENTIFIER = "TV0001";

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

            context.RegisterSyntaxNodeAction(
                AnalyzeAnonymousFunction,
                SyntaxKind.SimpleLambdaExpression,
                SyntaxKind.ParenthesizedLambdaExpression,
                SyntaxKind.AnonymousMethodExpression);
        }

        private static void AnalyzeAnonymousFunction(SyntaxNodeAnalysisContext context)
        {
            Location location;
            if (context.Node is LambdaExpressionSyntax lambda)
            {
                location = lambda.ArrowToken.GetLocation();
            }
            else
            {
                AnonymousMethodExpressionSyntax anonymousMethod = (AnonymousMethodExpressionSyntax)context.Node;
                location = anonymousMethod.DelegateKeyword.GetLocation();
            }

            context.ReportDiagnostic(Diagnostic.Create(_rule, location));
        }
    }
}
