using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace TopView.Analyzers
{
    /// <summary>
    /// 쓰지 않는 using 지시문을 찾는다.
    /// 파일의 이름이 가리키는 타입과 확장 메서드의 네임스페이스를 모아, 그 안에 없는 using을 경고한다.
    /// 별칭 using(<c>using A = …;</c>)과 <c>using static</c>은 검사하지 않는다.
    /// 컴파일러의 CS8019는 문서 생성(/doc) 없이 컴파일하는 Unity에서 나오지 않아 직접 계산한다.
    /// 같은 이유로 Unity 컴파일에서는 문서 주석의 cref가 해석되지 않아, cref에서만 쓰는 using도 경고된다.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class UnnecessaryUsingAnalyzer : DiagnosticAnalyzer
    {
        private static readonly DiagnosticDescriptor _rule = new DiagnosticDescriptor(
            DIAGNOSTIC_IDENTIFIER,
            "불필요한 using",
            "쓰지 않는 using이다. 지운다.",
            "Style",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public const string DIAGNOSTIC_IDENTIFIER = "TV0004";

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

            context.RegisterSemanticModelAction(AnalyzeSemanticModel);
        }

        private static void AnalyzeSemanticModel(SemanticModelAnalysisContext context)
        {
            SemanticModel semanticModel = context.SemanticModel;
            SyntaxNode root = semanticModel.SyntaxTree.GetRoot(context.CancellationToken);

            List<UsingDirectiveSyntax> usings = new List<UsingDirectiveSyntax>();
            foreach (SyntaxNode node in root.DescendantNodes())
            {
                if (node is UsingDirectiveSyntax usingDirective && usingDirective.Alias == null && !usingDirective.StaticKeyword.IsKind(SyntaxKind.StaticKeyword))
                {
                    usings.Add(usingDirective);
                }
            }

            if (usings.Count == 0)
            {
                return;
            }

            HashSet<string> usedNamespaces = CollectUsedNamespaces(semanticModel, root, context);

            foreach (UsingDirectiveSyntax usingDirective in usings)
            {
                if (!(semanticModel.GetSymbolInfo(usingDirective.Name, context.CancellationToken).Symbol is INamespaceSymbol namespaceSymbol))
                {
                    continue;
                }

                if (usedNamespaces.Contains(namespaceSymbol.ToDisplayString()))
                {
                    continue;
                }

                context.ReportDiagnostic(Diagnostic.Create(_rule, usingDirective.GetLocation()));
            }
        }

        private static HashSet<string> CollectUsedNamespaces(SemanticModel semanticModel, SyntaxNode root, SemanticModelAnalysisContext context)
        {
            HashSet<string> usedNamespaces = new HashSet<string>();
            foreach (SyntaxNode node in root.DescendantNodes(descendIntoTrivia: true))
            {
                if (!(node is SimpleNameSyntax name) || IsInsideUsingDirective(name))
                {
                    continue;
                }

                bool isQualified = IsQualified(name);
                SymbolInfo symbolInfo = semanticModel.GetSymbolInfo(name, context.CancellationToken);
                AddNamespace(usedNamespaces, symbolInfo.Symbol, isQualified);
                foreach (ISymbol candidate in symbolInfo.CandidateSymbols)
                {
                    AddNamespace(usedNamespaces, candidate, isQualified);
                }
            }

            return usedNamespaces;
        }

        private static void AddNamespace(HashSet<string> usedNamespaces, ISymbol symbol, bool isQualified)
        {
            string namespaceName = GetImportedNamespaceName(symbol, isQualified);
            if (namespaceName != null)
            {
                usedNamespaces.Add(namespaceName);
            }
        }

        private static string GetImportedNamespaceName(ISymbol symbol, bool isQualified)
        {
            if (symbol is IMethodSymbol method)
            {
                if (method.ReducedFrom != null)
                {
                    return GetNamespaceName(method.ReducedFrom.ContainingType.ContainingNamespace);
                }

                if (!isQualified && method.MethodKind == MethodKind.Constructor)
                {
                    return GetNamespaceName(method.ContainingType.ContainingNamespace);
                }

                return null;
            }

            if (isQualified)
            {
                return null;
            }

            if (symbol is INamedTypeSymbol type)
            {
                return GetNamespaceName(type.ContainingNamespace);
            }

            if (symbol is INamespaceSymbol namespaceSymbol)
            {
                return GetNamespaceName(namespaceSymbol.ContainingNamespace);
            }

            return null;
        }

        private static string GetNamespaceName(INamespaceSymbol namespaceSymbol)
        {
            if (namespaceSymbol == null || namespaceSymbol.IsGlobalNamespace)
            {
                return null;
            }

            return namespaceSymbol.ToDisplayString();
        }

        private static bool IsQualified(SimpleNameSyntax name)
        {
            SyntaxNode parent = name.Parent;
            if (parent is QualifiedNameSyntax qualifiedName)
            {
                return qualifiedName.Right == name;
            }

            if (parent is MemberAccessExpressionSyntax memberAccess)
            {
                return memberAccess.Name == name;
            }

            if (parent is AliasQualifiedNameSyntax aliasQualifiedName)
            {
                return aliasQualifiedName.Name == name;
            }

            if (parent is MemberBindingExpressionSyntax)
            {
                return true;
            }

            if (parent is NameMemberCrefSyntax nameMemberCref && nameMemberCref.Parent is QualifiedCrefSyntax qualifiedCref)
            {
                return qualifiedCref.Member == nameMemberCref;
            }

            return false;
        }

        private static bool IsInsideUsingDirective(SyntaxNode node)
        {
            SyntaxNode current = node.Parent;
            while (current != null)
            {
                if (current is UsingDirectiveSyntax)
                {
                    return true;
                }

                if (current is MemberDeclarationSyntax)
                {
                    return false;
                }

                current = current.Parent;
            }

            return false;
        }
    }
}
