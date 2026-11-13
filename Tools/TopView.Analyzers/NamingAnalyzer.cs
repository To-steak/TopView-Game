using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace TopView.Analyzers
{
    /// <summary>
    /// 이름 규칙을 검사한다: 대소문자 형식(TV0007), bool 접두사(TV0008), 줄임말(TV0009), 한 글자 이름(TV0010), 머리글자 약어(TV0011).
    /// 상수와 static readonly 필드는 UPPER_SNAKE_CASE로 보고, 머리글자 약어 검사를 하지 않는다.
    /// 외부 API가 이름을 정한 심볼(재정의, 인터페이스 구현, Unity 메시지와 그 매개변수)과 제네릭 타입 매개변수는 검사하지 않는다.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class NamingAnalyzer : DiagnosticAnalyzer
    {
        private static readonly DiagnosticDescriptor _caseRule = new DiagnosticDescriptor(
            CASE_DIAGNOSTIC_IDENTIFIER,
            "이름 형식",
            "'{0}'의 이름 형식이 틀렸다. {1}로 짓는다.",
            "Naming",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        private static readonly DiagnosticDescriptor _booleanPrefixRule = new DiagnosticDescriptor(
            BOOLEAN_PREFIX_DIAGNOSTIC_IDENTIFIER,
            "bool 접두사",
            "bool '{0}'에 접두사가 없다. is, has, can 중 하나로 시작한다.",
            "Naming",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        private static readonly DiagnosticDescriptor _abbreviationRule = new DiagnosticDescriptor(
            ABBREVIATION_DIAGNOSTIC_IDENTIFIER,
            "줄임말 금지",
            "'{0}'에 줄임말 '{1}'이(가) 있다. 단어를 줄이지 않고 쓴다.",
            "Naming",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        private static readonly DiagnosticDescriptor _singleLetterRule = new DiagnosticDescriptor(
            SINGLE_LETTER_DIAGNOSTIC_IDENTIFIER,
            "한 글자 이름 금지",
            "'{0}'은(는) 한 글자 이름이다. 무엇을 담는지 드러나는 단어로 짓는다.",
            "Naming",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        private static readonly DiagnosticDescriptor _acronymRule = new DiagnosticDescriptor(
            ACRONYM_DIAGNOSTIC_IDENTIFIER,
            "머리글자 약어 금지",
            "'{0}'에 머리글자 약어가 있다. 단어를 풀어 쓴다.",
            "Naming",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public const string CASE_DIAGNOSTIC_IDENTIFIER = "TV0007";
        public const string BOOLEAN_PREFIX_DIAGNOSTIC_IDENTIFIER = "TV0008";
        public const string ABBREVIATION_DIAGNOSTIC_IDENTIFIER = "TV0009";
        public const string SINGLE_LETTER_DIAGNOSTIC_IDENTIFIER = "TV0010";
        public const string ACRONYM_DIAGNOSTIC_IDENTIFIER = "TV0011";
        private const string PASCAL_CASE = "PascalCase";
        private const string CAMEL_CASE = "camelCase";
        private const string PRIVATE_FIELD_CASE = "_camelCase";
        private const string UPPER_SNAKE_CASE = "UPPER_SNAKE_CASE";

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(
            _caseRule, _booleanPrefixRule, _abbreviationRule, _singleLetterRule, _acronymRule);

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

            context.RegisterSymbolAction(AnalyzeSymbol, SymbolKind.NamedType, SymbolKind.Method, SymbolKind.Field, SymbolKind.Property, SymbolKind.Event);
            context.RegisterSyntaxNodeAction(
                AnalyzeLocalDeclaration,
                SyntaxKind.VariableDeclarator,
                SyntaxKind.SingleVariableDesignation,
                SyntaxKind.ForEachStatement,
                SyntaxKind.CatchDeclaration,
                SyntaxKind.LocalFunctionStatement);
        }

        private static void AnalyzeSymbol(SymbolAnalysisContext context)
        {
            ISymbol symbol = context.Symbol;
            if (symbol.IsImplicitlyDeclared || symbol.Locations.Length == 0 || !symbol.Locations[0].IsInSource)
            {
                return;
            }

            switch (symbol)
            {
                case INamedTypeSymbol type:
                    AnalyzeType(context, type);
                    break;
                case IMethodSymbol method:
                    AnalyzeMethod(context.ReportDiagnostic, method);
                    break;
                case IFieldSymbol field:
                    AnalyzeField(context, field);
                    break;
                case IPropertySymbol property:
                    AnalyzeProperty(context, property);
                    break;
                case IEventSymbol eventSymbol:
                    if (!NamingUtility.IsExternallyNamed(eventSymbol))
                    {
                        CheckName(context.ReportDiagnostic, eventSymbol, PASCAL_CASE, false);
                    }

                    break;
            }
        }

        private static void AnalyzeType(SymbolAnalysisContext context, INamedTypeSymbol type)
        {
            string checkedName = type.Name;
            bool isInterfacePrefix = type.TypeKind == TypeKind.Interface && checkedName.Length > 1 && checkedName[0] == 'I' && char.IsUpper(checkedName[1]);
            if (isInterfacePrefix)
            {
                checkedName = checkedName.Substring(1);
            }

            Location location = type.Locations[0];
            ReportCase(context.ReportDiagnostic, type.Name, location, NamingUtility.IsPascalCase(type.Name), PASCAL_CASE);
            CheckWords(context.ReportDiagnostic, type.Name, checkedName, location, true);
        }

        private static void AnalyzeMethod(System.Action<Diagnostic> report, IMethodSymbol method)
        {
            bool isExternallyNamed = NamingUtility.IsExternallyNamed(method);
            bool isNamedMethod = method.MethodKind == MethodKind.Ordinary || method.MethodKind == MethodKind.LocalFunction;
            if (isNamedMethod && !isExternallyNamed)
            {
                CheckName(report, method, PASCAL_CASE, false);
            }

            bool hasCheckedParameters = isNamedMethod || method.MethodKind == MethodKind.Constructor;
            if (!hasCheckedParameters || isExternallyNamed)
            {
                return;
            }

            foreach (IParameterSymbol parameter in method.Parameters)
            {
                if (parameter.Locations.Length == 0 || parameter.Name == "_")
                {
                    continue;
                }

                CheckName(report, parameter, CAMEL_CASE, IsBoolean(parameter.Type));
            }
        }

        private static void AnalyzeField(SymbolAnalysisContext context, IFieldSymbol field)
        {
            if (field.ContainingType.TypeKind == TypeKind.Enum)
            {
                CheckName(context.ReportDiagnostic, field, PASCAL_CASE, false);
                return;
            }

            // static readonly 필드는 프로젝트에서 상수처럼 쓰므로 상수와 같은 형식으로 짓는다
            if (field.IsConst || (field.IsStatic && field.IsReadOnly))
            {
                CheckName(context.ReportDiagnostic, field, UPPER_SNAKE_CASE, false);
                return;
            }

            string expectedCase = field.DeclaredAccessibility == Accessibility.Private ? PRIVATE_FIELD_CASE : CAMEL_CASE;
            CheckName(context.ReportDiagnostic, field, expectedCase, IsBoolean(field.Type));
        }

        private static void AnalyzeProperty(SymbolAnalysisContext context, IPropertySymbol property)
        {
            if (property.IsIndexer || NamingUtility.IsExternallyNamed(property))
            {
                return;
            }

            CheckName(context.ReportDiagnostic, property, PASCAL_CASE, IsBoolean(property.Type));
        }

        private static void AnalyzeLocalDeclaration(SyntaxNodeAnalysisContext context)
        {
            ISymbol symbol = context.SemanticModel.GetDeclaredSymbol(context.Node, context.CancellationToken);
            if (symbol is IMethodSymbol localFunction && localFunction.MethodKind == MethodKind.LocalFunction)
            {
                AnalyzeMethod(context.ReportDiagnostic, localFunction);
                return;
            }

            if (!(symbol is ILocalSymbol local) || local.Name == "_" || local.Locations.Length == 0)
            {
                return;
            }

            string expectedCase = local.IsConst ? UPPER_SNAKE_CASE : CAMEL_CASE;
            bool isBooleanChecked = !local.IsConst && IsBoolean(local.Type);
            CheckName(context.ReportDiagnostic, local, expectedCase, isBooleanChecked);
        }

        private static void CheckName(System.Action<Diagnostic> report, ISymbol symbol, string expectedCase, bool isBoolean)
        {
            string name = symbol.Name;
            Location location = symbol.Locations[0];
            ReportCase(report, name, location, MatchesCase(name, expectedCase), expectedCase);

            if (isBoolean && !NamingUtility.HasBooleanPrefix(name))
            {
                report(Diagnostic.Create(_booleanPrefixRule, location, name));
            }

            CheckWords(report, name, name, location, expectedCase != UPPER_SNAKE_CASE);
        }

        private static void CheckWords(System.Action<Diagnostic> report, string name, string checkedName, Location location, bool isAcronymChecked)
        {
            string abbreviation = NamingUtility.FindAbbreviation(checkedName, isAcronymChecked);
            if (abbreviation != null)
            {
                report(Diagnostic.Create(_abbreviationRule, location, name, abbreviation));
            }

            if (checkedName.TrimStart('_').Length == 1)
            {
                report(Diagnostic.Create(_singleLetterRule, location, name));
            }

            if (isAcronymChecked && NamingUtility.HasAcronym(checkedName))
            {
                report(Diagnostic.Create(_acronymRule, location, name));
            }
        }

        private static void ReportCase(System.Action<Diagnostic> report, string name, Location location, bool isMatched, string expectedCase)
        {
            if (!isMatched)
            {
                report(Diagnostic.Create(_caseRule, location, name, expectedCase));
            }
        }

        private static bool MatchesCase(string name, string expectedCase)
        {
            switch (expectedCase)
            {
                case PASCAL_CASE:
                    return NamingUtility.IsPascalCase(name);
                case CAMEL_CASE:
                    return NamingUtility.IsCamelCase(name);
                case PRIVATE_FIELD_CASE:
                    return NamingUtility.IsPrivateFieldCase(name);
                default:
                    return NamingUtility.IsUpperSnakeCase(name);
            }
        }

        private static bool IsBoolean(ITypeSymbol type)
        {
            return type != null && type.SpecialType == SpecialType.System_Boolean;
        }
    }
}
