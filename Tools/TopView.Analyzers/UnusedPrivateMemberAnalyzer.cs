using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace TopView.Analyzers
{
    /// <summary>
    /// 선언한 타입 안 어디에서도 참조하지 않는 private 멤버(필드, 상수, 메서드, 프로퍼티, 이벤트)를 찾는다.
    /// 속성이 붙은 멤버와 Unity 메시지 메서드는 리플렉션으로 쓰일 수 있어 검사하지 않는다.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class UnusedPrivateMemberAnalyzer : DiagnosticAnalyzer
    {
        private static readonly DiagnosticDescriptor _rule = new DiagnosticDescriptor(
            DIAGNOSTIC_IDENTIFIER,
            "안 쓰는 private 멤버",
            "private 멤버 '{0}'을(를) 아무 데서도 쓰지 않는다. 지운다.",
            "Style",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public const string DIAGNOSTIC_IDENTIFIER = "TV0005";

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

            context.RegisterSymbolStartAction(OnTypeStart, SymbolKind.NamedType);
        }

        private static void OnTypeStart(SymbolStartAnalysisContext context)
        {
            UnusedPrivateMemberTracker tracker = new UnusedPrivateMemberTracker((INamedTypeSymbol)context.Symbol, _rule);
            if (!tracker.HasCandidates)
            {
                return;
            }

            context.RegisterSyntaxNodeAction(tracker.AnalyzeName, SyntaxKind.IdentifierName, SyntaxKind.GenericName);
            context.RegisterSymbolEndAction(tracker.ReportUnused);
        }
    }
}
