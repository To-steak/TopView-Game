using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace TopView.Analyzers
{
    /// <summary>
    /// 다른 타입 안에 선언한 타입(클래스, struct, enum, 인터페이스, 델리게이트)을 찾는다.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class NestedTypeAnalyzer : DiagnosticAnalyzer
    {
        private static readonly DiagnosticDescriptor _rule = new DiagnosticDescriptor(
            DIAGNOSTIC_IDENTIFIER,
            "중첩 타입 금지",
            "'{0}'을(를) 다른 타입 안에 두지 않는다. 자기 이름의 파일로 꺼낸다.",
            "Style",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public const string DIAGNOSTIC_IDENTIFIER = "TV0003";

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

            context.RegisterSymbolAction(AnalyzeNamedType, SymbolKind.NamedType);
        }

        private static void AnalyzeNamedType(SymbolAnalysisContext context)
        {
            INamedTypeSymbol type = (INamedTypeSymbol)context.Symbol;
            if (type.ContainingType == null || type.IsImplicitlyDeclared)
            {
                return;
            }

            foreach (Location location in type.Locations)
            {
                context.ReportDiagnostic(Diagnostic.Create(_rule, location, type.Name));
            }
        }
    }
}
