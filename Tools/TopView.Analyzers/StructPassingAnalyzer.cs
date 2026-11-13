using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace TopView.Analyzers
{
    /// <summary>
    /// 프로젝트 struct를 복사 없이 넘기는지 검사한다: 배열 원소를 값으로 꺼냄(TV0020), struct 매개변수를 값으로 받음(TV0021),
    /// 고치지 않는 ref 매개변수(TV0022). 외부 API가 시그니처를 정한 메서드(재정의, 인터페이스 구현, Unity 메시지)와 virtual·abstract 메서드는 매개변수를 검사하지 않는다.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class StructPassingAnalyzer : DiagnosticAnalyzer
    {
        private static readonly DiagnosticDescriptor _arrayElementRule = new DiagnosticDescriptor(
            ARRAY_ELEMENT_DIAGNOSTIC_IDENTIFIER,
            "배열 원소는 ref로",
            "배열 원소 '{0}'을(를) 값으로 복사한다. ref로 꺼내고, 읽기만 하면 ref readonly로 꺼낸다.",
            "Style",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        private static readonly DiagnosticDescriptor _valueParameterRule = new DiagnosticDescriptor(
            VALUE_PARAMETER_DIAGNOSTIC_IDENTIFIER,
            "struct 매개변수는 ref나 in으로",
            "struct 매개변수 '{0}'을(를) 값으로 받는다. 고치면 ref, 읽기만 하면 in으로 받는다.",
            "Style",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        private static readonly DiagnosticDescriptor _unmodifiedReferenceRule = new DiagnosticDescriptor(
            UNMODIFIED_REFERENCE_DIAGNOSTIC_IDENTIFIER,
            "고치지 않는 ref 매개변수",
            "ref 매개변수 '{0}'을(를) 고치지 않는다. in으로 받는다.",
            "Style",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public const string ARRAY_ELEMENT_DIAGNOSTIC_IDENTIFIER = "TV0020";
        public const string VALUE_PARAMETER_DIAGNOSTIC_IDENTIFIER = "TV0021";
        public const string UNMODIFIED_REFERENCE_DIAGNOSTIC_IDENTIFIER = "TV0022";

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(
            _arrayElementRule, _valueParameterRule, _unmodifiedReferenceRule);

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

            context.RegisterSyntaxNodeAction(AnalyzeVariableDeclarator, SyntaxKind.VariableDeclarator);
            context.RegisterSyntaxNodeAction(AnalyzeForEach, SyntaxKind.ForEachStatement);
            context.RegisterSymbolAction(AnalyzeMethodSignature, SymbolKind.Method);
            context.RegisterOperationBlockAction(AnalyzeMethodBody);
        }

        private static void AnalyzeVariableDeclarator(SyntaxNodeAnalysisContext context)
        {
            VariableDeclaratorSyntax declarator = (VariableDeclaratorSyntax)context.Node;
            if (!(declarator.Parent is VariableDeclarationSyntax declaration) || !(declaration.Parent is LocalDeclarationStatementSyntax))
            {
                return;
            }

            if (declaration.Type is RefTypeSyntax || declarator.Initializer == null)
            {
                return;
            }

            if (!(declarator.Initializer.Value is ElementAccessExpressionSyntax elementAccess))
            {
                return;
            }

            ITypeSymbol collectionType = context.SemanticModel.GetTypeInfo(elementAccess.Expression, context.CancellationToken).Type;
            if (collectionType is IArrayTypeSymbol arrayType && AnalyzerScopeUtility.IsDataStruct(arrayType.ElementType))
            {
                context.ReportDiagnostic(Diagnostic.Create(_arrayElementRule, declarator.Identifier.GetLocation(), declarator.Identifier.ValueText));
            }
        }

        private static void AnalyzeForEach(SyntaxNodeAnalysisContext context)
        {
            ForEachStatementSyntax forEach = (ForEachStatementSyntax)context.Node;
            if (forEach.Type is RefTypeSyntax)
            {
                return;
            }

            ITypeSymbol collectionType = context.SemanticModel.GetTypeInfo(forEach.Expression, context.CancellationToken).Type;
            if (collectionType is IArrayTypeSymbol arrayType && AnalyzerScopeUtility.IsDataStruct(arrayType.ElementType))
            {
                context.ReportDiagnostic(Diagnostic.Create(_arrayElementRule, forEach.Identifier.GetLocation(), forEach.Identifier.ValueText));
            }
        }

        private static void AnalyzeMethodSignature(SymbolAnalysisContext context)
        {
            IMethodSymbol method = (IMethodSymbol)context.Symbol;
            if (!HasCheckedParameters(method))
            {
                return;
            }

            foreach (IParameterSymbol parameter in method.Parameters)
            {
                if (parameter.RefKind == RefKind.None && AnalyzerScopeUtility.IsProjectStruct(parameter.Type) && parameter.Locations.Length > 0)
                {
                    context.ReportDiagnostic(Diagnostic.Create(_valueParameterRule, parameter.Locations[0], parameter.Name));
                }
            }
        }

        private static void AnalyzeMethodBody(OperationBlockAnalysisContext context)
        {
            if (!(context.OwningSymbol is IMethodSymbol method) || !HasCheckedParameters(method))
            {
                return;
            }

            HashSet<IParameterSymbol> unmodifiedParameters = new HashSet<IParameterSymbol>(SymbolEqualityComparer.Default);
            foreach (IParameterSymbol parameter in method.Parameters)
            {
                if (parameter.RefKind == RefKind.Ref && AnalyzerScopeUtility.IsProjectStruct(parameter.Type))
                {
                    unmodifiedParameters.Add(parameter);
                }
            }

            if (unmodifiedParameters.Count == 0)
            {
                return;
            }

            foreach (IOperation block in context.OperationBlocks)
            {
                foreach (IOperation operation in block.Descendants())
                {
                    if (operation is IParameterReferenceOperation reference
                        && unmodifiedParameters.Contains(reference.Parameter)
                        && WriteDetectionUtility.IsWrite(reference, reference.Parameter.Type))
                    {
                        unmodifiedParameters.Remove(reference.Parameter);
                    }
                }
            }

            foreach (IParameterSymbol parameter in unmodifiedParameters)
            {
                context.ReportDiagnostic(Diagnostic.Create(_unmodifiedReferenceRule, parameter.Locations[0], parameter.Name));
            }
        }

        private static bool HasCheckedParameters(IMethodSymbol method)
        {
            if (method.IsImplicitlyDeclared || method.IsVirtual || method.IsAbstract || NamingUtility.IsExternallyNamed(method))
            {
                return false;
            }

            switch (method.MethodKind)
            {
                case MethodKind.Ordinary:
                case MethodKind.Constructor:
                case MethodKind.LocalFunction:
                    return true;
                default:
                    return false;
            }
        }
    }
}
