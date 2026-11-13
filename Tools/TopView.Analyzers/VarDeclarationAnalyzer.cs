using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace TopView.Analyzers
{
    /// <summary>
    /// <c>var</c>로 선언한 변수를 찾는다. 지역 변수, foreach, 분해 선언, <c>out var</c>, <c>is var</c> 패턴을 모두 검사한다.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class VarDeclarationAnalyzer : DiagnosticAnalyzer
    {
        private static readonly DiagnosticDescriptor _rule = new DiagnosticDescriptor(
            DIAGNOSTIC_IDENTIFIER,
            "var 금지",
            "var를 쓰지 않는다. 타입을 명시한다.",
            "Style",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public const string DIAGNOSTIC_IDENTIFIER = "TV0002";

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

            context.RegisterSyntaxNodeAction(AnalyzeIdentifier, SyntaxKind.IdentifierName);
            context.RegisterSyntaxNodeAction(AnalyzeVarPattern, SyntaxKind.VarPattern);
        }

        private static void AnalyzeIdentifier(SyntaxNodeAnalysisContext context)
        {
            IdentifierNameSyntax identifier = (IdentifierNameSyntax)context.Node;
            if (!identifier.IsVar)
            {
                return;
            }

            if (!IsDeclarationType(identifier))
            {
                return;
            }

            context.ReportDiagnostic(Diagnostic.Create(_rule, identifier.GetLocation()));
        }

        private static void AnalyzeVarPattern(SyntaxNodeAnalysisContext context)
        {
            VarPatternSyntax pattern = (VarPatternSyntax)context.Node;
            context.ReportDiagnostic(Diagnostic.Create(_rule, pattern.VarKeyword.GetLocation()));
        }

        private static bool IsDeclarationType(IdentifierNameSyntax identifier)
        {
            SyntaxNode parent = identifier.Parent;
            if (parent is VariableDeclarationSyntax variableDeclaration)
            {
                return variableDeclaration.Type == identifier;
            }

            if (parent is ForEachStatementSyntax forEachStatement)
            {
                return forEachStatement.Type == identifier;
            }

            if (parent is DeclarationExpressionSyntax declarationExpression)
            {
                return declarationExpression.Type == identifier;
            }

            return false;
        }
    }
}
