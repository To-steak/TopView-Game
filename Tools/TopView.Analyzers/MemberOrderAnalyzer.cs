using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace TopView.Analyzers
{
    /// <summary>
    /// 클래스·struct 선언 안의 멤버 순서를 검사한다.
    /// public 필드 → public 프로퍼티 → [SerializeField] private 필드 → private 필드 → private 프로퍼티 → const → public 생성자 → public 메서드 → private 메서드.
    /// 이 아홉 종류에 들지 않는 멤버(internal·protected 멤버, 이벤트, private 생성자, 인덱서, 연산자, 명시적 인터페이스 구현)는 검사하지 않는다.
    /// partial 타입은 파일마다 따로 검사한다.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class MemberOrderAnalyzer : DiagnosticAnalyzer
    {
        private static readonly DiagnosticDescriptor _rule = new DiagnosticDescriptor(
            DIAGNOSTIC_IDENTIFIER,
            "멤버 순서",
            "'{0}'의 자리가 틀렸다. {1} 묶음은 {2} 묶음보다 앞에 둔다.",
            "Style",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        private static readonly string[] GROUP_NAMES =
        {
            "검사하지 않는 멤버",
            "public 필드",
            "public 프로퍼티",
            "[SerializeField] private 필드",
            "private 필드",
            "private 프로퍼티",
            "const",
            "public 생성자",
            "public 메서드",
            "private 메서드",
        };

        public const string DIAGNOSTIC_IDENTIFIER = "TV0018";
        private const int UNCHECKED_GROUP = 0;
        private const int PUBLIC_FIELD_GROUP = 1;
        private const int PUBLIC_PROPERTY_GROUP = 2;
        private const int SERIALIZED_FIELD_GROUP = 3;
        private const int PRIVATE_FIELD_GROUP = 4;
        private const int PRIVATE_PROPERTY_GROUP = 5;
        private const int CONSTANT_GROUP = 6;
        private const int PUBLIC_CONSTRUCTOR_GROUP = 7;
        private const int PUBLIC_METHOD_GROUP = 8;
        private const int PRIVATE_METHOD_GROUP = 9;
        private const string SERIALIZE_FIELD_ATTRIBUTE = "SerializeField";

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

            context.RegisterSyntaxNodeAction(AnalyzeType, SyntaxKind.ClassDeclaration, SyntaxKind.StructDeclaration);
        }

        private static void AnalyzeType(SyntaxNodeAnalysisContext context)
        {
            TypeDeclarationSyntax type = (TypeDeclarationSyntax)context.Node;
            int highestGroup = UNCHECKED_GROUP;
            foreach (MemberDeclarationSyntax member in type.Members)
            {
                int group = GetGroup(member);
                if (group == UNCHECKED_GROUP)
                {
                    continue;
                }

                if (group < highestGroup)
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                        _rule, GetNameLocation(member), GetMemberName(member), GROUP_NAMES[group], GROUP_NAMES[highestGroup]));
                    continue;
                }

                highestGroup = group;
            }
        }

        private static int GetGroup(MemberDeclarationSyntax member)
        {
            switch (member)
            {
                case FieldDeclarationSyntax field:
                    if (HasModifier(field.Modifiers, SyntaxKind.ConstKeyword))
                    {
                        return CONSTANT_GROUP;
                    }

                    if (HasModifier(field.Modifiers, SyntaxKind.PublicKeyword))
                    {
                        return PUBLIC_FIELD_GROUP;
                    }

                    if (!IsPrivate(field.Modifiers))
                    {
                        return UNCHECKED_GROUP;
                    }

                    return HasAttribute(field.AttributeLists, SERIALIZE_FIELD_ATTRIBUTE) ? SERIALIZED_FIELD_GROUP : PRIVATE_FIELD_GROUP;
                case PropertyDeclarationSyntax property:
                    if (property.ExplicitInterfaceSpecifier != null)
                    {
                        return UNCHECKED_GROUP;
                    }

                    if (HasModifier(property.Modifiers, SyntaxKind.PublicKeyword))
                    {
                        return PUBLIC_PROPERTY_GROUP;
                    }

                    return IsPrivate(property.Modifiers) ? PRIVATE_PROPERTY_GROUP : UNCHECKED_GROUP;
                case ConstructorDeclarationSyntax constructor:
                    bool isPublicInstance = HasModifier(constructor.Modifiers, SyntaxKind.PublicKeyword) && !HasModifier(constructor.Modifiers, SyntaxKind.StaticKeyword);
                    return isPublicInstance ? PUBLIC_CONSTRUCTOR_GROUP : UNCHECKED_GROUP;
                case MethodDeclarationSyntax method:
                    if (method.ExplicitInterfaceSpecifier != null)
                    {
                        return UNCHECKED_GROUP;
                    }

                    if (HasModifier(method.Modifiers, SyntaxKind.PublicKeyword))
                    {
                        return PUBLIC_METHOD_GROUP;
                    }

                    return IsPrivate(method.Modifiers) ? PRIVATE_METHOD_GROUP : UNCHECKED_GROUP;
                default:
                    return UNCHECKED_GROUP;
            }
        }

        private static bool IsPrivate(SyntaxTokenList modifiers)
        {
            if (HasModifier(modifiers, SyntaxKind.PrivateKeyword))
            {
                return !HasModifier(modifiers, SyntaxKind.ProtectedKeyword);
            }

            return !HasModifier(modifiers, SyntaxKind.PublicKeyword)
                && !HasModifier(modifiers, SyntaxKind.InternalKeyword)
                && !HasModifier(modifiers, SyntaxKind.ProtectedKeyword);
        }

        private static bool HasModifier(SyntaxTokenList modifiers, SyntaxKind kind)
        {
            foreach (SyntaxToken modifier in modifiers)
            {
                if (modifier.IsKind(kind))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool HasAttribute(SyntaxList<AttributeListSyntax> attributeLists, string attributeName)
        {
            foreach (AttributeListSyntax attributeList in attributeLists)
            {
                foreach (AttributeSyntax attribute in attributeList.Attributes)
                {
                    string name = attribute.Name.ToString();
                    int separatorIndex = name.LastIndexOf('.');
                    if (separatorIndex >= 0)
                    {
                        name = name.Substring(separatorIndex + 1);
                    }

                    if (name == attributeName || name == attributeName + "Attribute")
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static string GetMemberName(MemberDeclarationSyntax member)
        {
            switch (member)
            {
                case FieldDeclarationSyntax field:
                    return field.Declaration.Variables[0].Identifier.ValueText;
                case PropertyDeclarationSyntax property:
                    return property.Identifier.ValueText;
                case ConstructorDeclarationSyntax constructor:
                    return constructor.Identifier.ValueText;
                case MethodDeclarationSyntax method:
                    return method.Identifier.ValueText;
                default:
                    return string.Empty;
            }
        }

        private static Location GetNameLocation(MemberDeclarationSyntax member)
        {
            switch (member)
            {
                case FieldDeclarationSyntax field:
                    return field.Declaration.Variables[0].Identifier.GetLocation();
                case PropertyDeclarationSyntax property:
                    return property.Identifier.GetLocation();
                case ConstructorDeclarationSyntax constructor:
                    return constructor.Identifier.GetLocation();
                case MethodDeclarationSyntax method:
                    return method.Identifier.GetLocation();
                default:
                    return member.GetLocation();
            }
        }
    }
}
