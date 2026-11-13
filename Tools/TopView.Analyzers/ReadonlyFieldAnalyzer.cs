using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace TopView.Analyzers
{
    /// <summary>
    /// 선언 초기화식이나 생성자에서만 값을 넣는 private 필드를 찾아 readonly를 권한다.
    /// 속성이 붙은 필드(<c>[SerializeField]</c> 등)는 Unity가 값을 넣으므로 검사하지 않는다.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class ReadonlyFieldAnalyzer : DiagnosticAnalyzer
    {
        private static readonly DiagnosticDescriptor _rule = new DiagnosticDescriptor(
            DIAGNOSTIC_IDENTIFIER,
            "readonly 권장",
            "private 필드 '{0}'은(는) 생성자 밖에서 바뀌지 않는다. readonly를 붙인다.",
            "Style",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public const string DIAGNOSTIC_IDENTIFIER = "TV0006";

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
            ReadonlyFieldTracker tracker = new ReadonlyFieldTracker((INamedTypeSymbol)context.Symbol, _rule);
            if (!tracker.HasCandidates)
            {
                return;
            }

            context.RegisterOperationAction(tracker.AnalyzeFieldReference, OperationKind.FieldReference);
            context.RegisterSymbolEndAction(tracker.ReportReadonlyCandidates);
        }
    }
}
