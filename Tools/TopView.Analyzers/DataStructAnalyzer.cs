using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace TopView.Analyzers
{
    /// <summary>
    /// 컴포넌트(<c>~Component</c>)와 엔티티(<c>~Entity</c>)가 struct이고 인스턴스 필드가 모두 public인지 검사한다.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class DataStructAnalyzer : DiagnosticAnalyzer
    {
        private static readonly DiagnosticDescriptor _rule = new DiagnosticDescriptor(
            DIAGNOSTIC_IDENTIFIER,
            "컴포넌트·엔티티는 struct와 public 필드",
            "{0}",
            "Style",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public const string DIAGNOSTIC_IDENTIFIER = "TV0019";

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

            context.RegisterSymbolAction(AnalyzeType, SymbolKind.NamedType);
        }

        private static void AnalyzeType(SymbolAnalysisContext context)
        {
            INamedTypeSymbol type = (INamedTypeSymbol)context.Symbol;
            if (!AnalyzerScopeUtility.HasDataSuffix(type.Name))
            {
                return;
            }

            if (type.TypeKind != TypeKind.Struct)
            {
                string message = $"'{type.Name}'은(는) 컴포넌트·엔티티라 struct로 만든다.";
                context.ReportDiagnostic(Diagnostic.Create(_rule, type.Locations[0], message));
                return;
            }

            foreach (ISymbol member in type.GetMembers())
            {
                if (!(member is IFieldSymbol field) || field.IsStatic || field.IsConst || field.IsImplicitlyDeclared)
                {
                    continue;
                }

                if (field.DeclaredAccessibility != Accessibility.Public)
                {
                    string message = $"'{type.Name}'의 필드 '{field.Name}'이(가) public이 아니다. 컴포넌트·엔티티의 필드는 public으로 둔다.";
                    context.ReportDiagnostic(Diagnostic.Create(_rule, field.Locations[0], message));
                }
            }
        }
    }
}
