using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace TopView.Analyzers
{
    /// <summary>
    /// 구 입력 API(<c>UnityEngine.Input</c>)를 쓰는 곳을 찾는다.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class LegacyInputAnalyzer : DiagnosticAnalyzer
    {
        private static readonly DiagnosticDescriptor _rule = new DiagnosticDescriptor(
            DIAGNOSTIC_IDENTIFIER,
            "구 Input API 금지",
            "구 입력 API UnityEngine.Input을 쓴다. 새 Input System으로 읽는다.",
            "Unity",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public const string DIAGNOSTIC_IDENTIFIER = "TV0025";
        private const string LEGACY_INPUT_TYPE_NAME = "UnityEngine.Input";
        private const string LEGACY_INPUT_SIMPLE_NAME = "Input";

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

            context.RegisterSyntaxNodeAction(AnalyzeName, SyntaxKind.IdentifierName);
        }

        private static void AnalyzeName(SyntaxNodeAnalysisContext context)
        {
            IdentifierNameSyntax name = (IdentifierNameSyntax)context.Node;
            if (name.Identifier.ValueText != LEGACY_INPUT_SIMPLE_NAME)
            {
                return;
            }

            ISymbol symbol = context.SemanticModel.GetSymbolInfo(name, context.CancellationToken).Symbol;
            if (symbol is INamedTypeSymbol type && type.ToDisplayString() == LEGACY_INPUT_TYPE_NAME)
            {
                context.ReportDiagnostic(Diagnostic.Create(_rule, name.GetLocation()));
            }
        }
    }
}
