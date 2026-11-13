using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace TopView.Analyzers
{
    /// <summary>
    /// 파일과 네임스페이스의 배치를 검사한다: 네임스페이스와 폴더(TV0012), 파일 하나에 타입 하나(TV0013),
    /// 파일 이름과 타입 이름(TV0014), partial 타입의 본 파일(TV0015), partial 파일의 폴더(TV0016).
    /// 스크립트 루트(<c>Assets/Scripts/</c>) 밖의 파일은 검사하지 않는다.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class FileLayoutAnalyzer : DiagnosticAnalyzer
    {
        private static readonly DiagnosticDescriptor _namespaceRule = new DiagnosticDescriptor(
            NAMESPACE_DIAGNOSTIC_IDENTIFIER,
            "네임스페이스와 폴더",
            "네임스페이스 '{0}'이(가) 폴더와 맞지 않는다. '{1}'로 둔다.",
            "Layout",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        private static readonly DiagnosticDescriptor _singleTypeRule = new DiagnosticDescriptor(
            SINGLE_TYPE_DIAGNOSTIC_IDENTIFIER,
            "파일 하나에 타입 하나",
            "'{0}'은(는) 이 파일의 두 번째 이후 타입이다. 자기 이름의 파일로 꺼낸다.",
            "Layout",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        private static readonly DiagnosticDescriptor _fileNameRule = new DiagnosticDescriptor(
            FILE_NAME_DIAGNOSTIC_IDENTIFIER,
            "파일 이름과 타입 이름",
            "파일 이름 '{0}'이(가) 타입 '{1}'과(와) 맞지 않는다. '{1}.cs'로 짓고, partial 파일이면 '{1}.역할.cs'로 짓는다.",
            "Layout",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        private static readonly DiagnosticDescriptor _partialMainFileRule = new DiagnosticDescriptor(
            PARTIAL_MAIN_FILE_DIAGNOSTIC_IDENTIFIER,
            "partial 본 파일",
            "partial 타입 '{0}'에 본 파일이 없다. '{0}.cs'를 둔다.",
            "Layout",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        private static readonly DiagnosticDescriptor _partialFolderRule = new DiagnosticDescriptor(
            PARTIAL_FOLDER_DIAGNOSTIC_IDENTIFIER,
            "partial 파일의 폴더",
            "partial 타입 '{0}'의 파일이 여러 폴더에 있다. 같은 폴더에 둔다.",
            "Layout",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public const string NAMESPACE_DIAGNOSTIC_IDENTIFIER = "TV0012";
        public const string SINGLE_TYPE_DIAGNOSTIC_IDENTIFIER = "TV0013";
        public const string FILE_NAME_DIAGNOSTIC_IDENTIFIER = "TV0014";
        public const string PARTIAL_MAIN_FILE_DIAGNOSTIC_IDENTIFIER = "TV0015";
        public const string PARTIAL_FOLDER_DIAGNOSTIC_IDENTIFIER = "TV0016";
        private const string COMPONENTS_FOLDER = "Components";
        private const string FILE_EXTENSION = ".cs";

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(
            _namespaceRule, _singleTypeRule, _fileNameRule, _partialMainFileRule, _partialFolderRule);

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
            context.RegisterSymbolAction(AnalyzePartialType, SymbolKind.NamedType);
        }

        private static void AnalyzeTree(SyntaxTreeAnalysisContext context)
        {
            string relativePath = AnalyzerScopeUtility.GetScriptRelativePath(context.Tree.FilePath);
            if (relativePath == null)
            {
                return;
            }

            CompilationUnitSyntax root = (CompilationUnitSyntax)context.Tree.GetRoot(context.CancellationToken);
            List<MemberDeclarationSyntax> types = new List<MemberDeclarationSyntax>();
            CollectTopLevelTypes(root.Members, types);
            if (types.Count == 0)
            {
                return;
            }

            string[] pathParts = relativePath.Split('/');
            string expectedNamespace = BuildExpectedNamespace(pathParts);
            ReportNamespaces(context, types, expectedNamespace);

            for (int index = 1; index < types.Count; index++)
            {
                SyntaxToken identifier = GetIdentifier(types[index]);
                context.ReportDiagnostic(Diagnostic.Create(_singleTypeRule, identifier.GetLocation(), identifier.ValueText));
            }

            string fileName = pathParts[pathParts.Length - 1];
            ReportFileName(context, types[0], fileName);
        }

        private static void AnalyzePartialType(SymbolAnalysisContext context)
        {
            INamedTypeSymbol type = (INamedTypeSymbol)context.Symbol;
            if (type.ContainingType != null || type.DeclaringSyntaxReferences.Length < 2)
            {
                return;
            }

            List<string> relativePaths = new List<string>();
            foreach (SyntaxReference reference in type.DeclaringSyntaxReferences)
            {
                string relativePath = AnalyzerScopeUtility.GetScriptRelativePath(reference.SyntaxTree.FilePath);
                if (relativePath != null)
                {
                    relativePaths.Add(relativePath);
                }
            }

            if (relativePaths.Count < 2)
            {
                return;
            }

            Location location = type.Locations[0];
            string mainFileName = type.Name + FILE_EXTENSION;
            bool hasMainFile = false;
            HashSet<string> folders = new HashSet<string>();
            foreach (string relativePath in relativePaths)
            {
                int separatorIndex = relativePath.LastIndexOf('/');
                string fileName = relativePath.Substring(separatorIndex + 1);
                folders.Add(separatorIndex < 0 ? string.Empty : relativePath.Substring(0, separatorIndex));
                if (fileName == mainFileName)
                {
                    hasMainFile = true;
                }
            }

            if (!hasMainFile)
            {
                context.ReportDiagnostic(Diagnostic.Create(_partialMainFileRule, location, type.Name));
            }

            if (folders.Count > 1)
            {
                context.ReportDiagnostic(Diagnostic.Create(_partialFolderRule, location, type.Name));
            }
        }

        private static void CollectTopLevelTypes(SyntaxList<MemberDeclarationSyntax> members, List<MemberDeclarationSyntax> types)
        {
            foreach (MemberDeclarationSyntax member in members)
            {
                if (member is NamespaceDeclarationSyntax namespaceDeclaration)
                {
                    CollectTopLevelTypes(namespaceDeclaration.Members, types);
                }
                else if (member is BaseTypeDeclarationSyntax || member is DelegateDeclarationSyntax)
                {
                    types.Add(member);
                }
            }
        }

        private static string BuildExpectedNamespace(string[] pathParts)
        {
            int folderCount = pathParts.Length - 1;
            if (folderCount > 0 && pathParts[0] == COMPONENTS_FOLDER)
            {
                return AnalyzerScopeUtility.RootNamespace + "." + COMPONENTS_FOLDER;
            }

            string expectedNamespace = AnalyzerScopeUtility.RootNamespace;
            for (int index = 0; index < folderCount; index++)
            {
                expectedNamespace += "." + pathParts[index].Replace(" ", string.Empty);
            }

            return expectedNamespace;
        }

        private static void ReportNamespaces(SyntaxTreeAnalysisContext context, List<MemberDeclarationSyntax> types, string expectedNamespace)
        {
            HashSet<SyntaxNode> reportedNamespaces = new HashSet<SyntaxNode>();
            foreach (MemberDeclarationSyntax type in types)
            {
                NamespaceDeclarationSyntax innermostNamespace = type.Parent as NamespaceDeclarationSyntax;
                string actualNamespace = GetFullNamespace(type);
                if (actualNamespace == expectedNamespace)
                {
                    continue;
                }

                if (innermostNamespace == null)
                {
                    SyntaxToken identifier = GetIdentifier(type);
                    context.ReportDiagnostic(Diagnostic.Create(_namespaceRule, identifier.GetLocation(), "(없음)", expectedNamespace));
                    continue;
                }

                if (reportedNamespaces.Add(innermostNamespace))
                {
                    context.ReportDiagnostic(Diagnostic.Create(_namespaceRule, innermostNamespace.Name.GetLocation(), actualNamespace, expectedNamespace));
                }
            }
        }

        private static string GetFullNamespace(SyntaxNode node)
        {
            string fullNamespace = string.Empty;
            SyntaxNode current = node.Parent;
            while (current != null)
            {
                if (current is NamespaceDeclarationSyntax namespaceDeclaration)
                {
                    string name = namespaceDeclaration.Name.ToString().Replace(" ", string.Empty);
                    fullNamespace = fullNamespace.Length == 0 ? name : name + "." + fullNamespace;
                }

                current = current.Parent;
            }

            return fullNamespace;
        }

        private static void ReportFileName(SyntaxTreeAnalysisContext context, MemberDeclarationSyntax type, string fileName)
        {
            SyntaxToken identifier = GetIdentifier(type);
            string typeName = identifier.ValueText;
            string fileStem = fileName.EndsWith(FILE_EXTENSION, StringComparison.Ordinal)
                ? fileName.Substring(0, fileName.Length - FILE_EXTENSION.Length)
                : fileName;

            if (fileStem == typeName)
            {
                return;
            }

            if (IsPartial(type) && fileStem.StartsWith(typeName + ".", StringComparison.Ordinal))
            {
                string role = fileStem.Substring(typeName.Length + 1);
                if (role.IndexOf('.') < 0 && NamingUtility.IsPascalCase(role))
                {
                    return;
                }
            }

            context.ReportDiagnostic(Diagnostic.Create(_fileNameRule, identifier.GetLocation(), fileName, typeName));
        }

        private static bool IsPartial(MemberDeclarationSyntax type)
        {
            foreach (SyntaxToken modifier in type.Modifiers)
            {
                if (modifier.IsKind(SyntaxKind.PartialKeyword))
                {
                    return true;
                }
            }

            return false;
        }

        private static SyntaxToken GetIdentifier(MemberDeclarationSyntax type)
        {
            if (type is BaseTypeDeclarationSyntax typeDeclaration)
            {
                return typeDeclaration.Identifier;
            }

            return ((DelegateDeclarationSyntax)type).Identifier;
        }
    }
}
