using System.Collections.Concurrent;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace TopView.Analyzers
{
    /// <summary>
    /// 타입 하나를 분석하는 동안 private 멤버가 참조되는지 기록한다.
    /// 참조가 보이면 후보에서 지우고, 타입 분석이 끝났을 때 남은 후보를 경고한다.
    /// </summary>
    public sealed class UnusedPrivateMemberTracker
    {
        private readonly DiagnosticDescriptor _rule;
        private readonly ConcurrentDictionary<ISymbol, bool> _unusedMembers;
        private readonly HashSet<string> _candidateNames;

        /// <summary>
        /// 타입의 멤버 중 검사할 private 멤버를 후보로 모은다.
        /// </summary>
        /// <param name="type">분석할 타입</param>
        /// <param name="rule">남은 후보에 낼 진단 규칙</param>
        public UnusedPrivateMemberTracker(INamedTypeSymbol type, DiagnosticDescriptor rule)
        {
            _rule = rule;
            _unusedMembers = new ConcurrentDictionary<ISymbol, bool>(SymbolEqualityComparer.Default);
            _candidateNames = new HashSet<string>();

            foreach (ISymbol member in type.GetMembers())
            {
                if (!IsCandidate(member))
                {
                    continue;
                }

                _unusedMembers.TryAdd(member, true);
                _candidateNames.Add(member.Name);
            }
        }

        public bool HasCandidates => _candidateNames.Count > 0;

        /// <summary>
        /// 이름 하나가 가리키는 멤버를 사용됨으로 기록한다.
        /// </summary>
        /// <param name="context">이름 노드의 분석 문맥</param>
        public void AnalyzeName(SyntaxNodeAnalysisContext context)
        {
            SimpleNameSyntax name = (SimpleNameSyntax)context.Node;
            if (!_candidateNames.Contains(name.Identifier.ValueText))
            {
                return;
            }

            SymbolInfo symbolInfo = context.SemanticModel.GetSymbolInfo(name, context.CancellationToken);
            MarkUsed(symbolInfo.Symbol);
            foreach (ISymbol candidate in symbolInfo.CandidateSymbols)
            {
                MarkUsed(candidate);
            }
        }

        /// <summary>
        /// 끝까지 참조되지 않은 멤버를 경고한다.
        /// </summary>
        /// <param name="context">타입 분석이 끝났을 때의 문맥</param>
        public void ReportUnused(SymbolAnalysisContext context)
        {
            foreach (ISymbol member in _unusedMembers.Keys)
            {
                context.ReportDiagnostic(Diagnostic.Create(_rule, member.Locations[0], member.Name));
            }
        }

        private void MarkUsed(ISymbol symbol)
        {
            if (symbol == null)
            {
                return;
            }

            ISymbol definition = symbol.OriginalDefinition;
            if (definition is IMethodSymbol method)
            {
                if (method.ReducedFrom != null)
                {
                    definition = method.ReducedFrom.OriginalDefinition;
                }
                else if (method.PartialDefinitionPart != null)
                {
                    definition = method.PartialDefinitionPart;
                }
            }

            _unusedMembers.TryRemove(definition, out bool _);
        }

        private static bool IsCandidate(ISymbol member)
        {
            if (member.DeclaredAccessibility != Accessibility.Private || member.IsImplicitlyDeclared)
            {
                return false;
            }

            if (member.GetAttributes().Length > 0)
            {
                return false;
            }

            switch (member)
            {
                case IFieldSymbol _:
                    return true;
                case IPropertySymbol property:
                    return property.ExplicitInterfaceImplementations.Length == 0;
                case IEventSymbol eventSymbol:
                    return eventSymbol.ExplicitInterfaceImplementations.Length == 0;
                case IMethodSymbol method:
                    return method.MethodKind == MethodKind.Ordinary
                        && method.ExplicitInterfaceImplementations.Length == 0
                        && !UnityMessageUtility.IsUnityMessage(method);
                default:
                    return false;
            }
        }
    }
}
