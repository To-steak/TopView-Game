using System.Collections.Concurrent;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace TopView.Analyzers
{
    /// <summary>
    /// 타입 하나를 분석하는 동안 private 필드에 생성자 밖에서 값을 쓰는지 기록한다.
    /// 생성자 밖의 쓰기가 보이면 후보에서 지우고, 타입 분석이 끝났을 때 남은 후보에 readonly를 권한다.
    /// </summary>
    public sealed class ReadonlyFieldTracker
    {
        private readonly DiagnosticDescriptor _rule;
        private readonly ConcurrentDictionary<IFieldSymbol, bool> _candidates;

        /// <summary>
        /// 타입의 필드 중 readonly가 될 수 있는 private 필드를 후보로 모은다.
        /// </summary>
        /// <param name="type">분석할 타입</param>
        /// <param name="rule">남은 후보에 낼 진단 규칙</param>
        public ReadonlyFieldTracker(INamedTypeSymbol type, DiagnosticDescriptor rule)
        {
            _rule = rule;
            _candidates = new ConcurrentDictionary<IFieldSymbol, bool>(SymbolEqualityComparer.Default);

            foreach (ISymbol member in type.GetMembers())
            {
                if (member is IFieldSymbol field && IsCandidate(field))
                {
                    _candidates.TryAdd(field, true);
                }
            }
        }

        public bool HasCandidates => !_candidates.IsEmpty;

        /// <summary>
        /// 필드 참조가 생성자 밖의 쓰기면 그 필드를 후보에서 지운다.
        /// </summary>
        /// <param name="context">필드 참조 연산의 분석 문맥</param>
        public void AnalyzeFieldReference(OperationAnalysisContext context)
        {
            IFieldReferenceOperation reference = (IFieldReferenceOperation)context.Operation;
            IFieldSymbol field = reference.Field.OriginalDefinition;
            if (!_candidates.ContainsKey(field))
            {
                return;
            }

            if (!WriteDetectionUtility.IsWrite(reference, field.Type))
            {
                return;
            }

            if (IsInitializingWrite(reference, field, context.ContainingSymbol))
            {
                return;
            }

            _candidates.TryRemove(field, out bool _);
        }

        /// <summary>
        /// 끝까지 생성자 밖에서 쓰이지 않은 필드에 readonly를 권한다.
        /// </summary>
        /// <param name="context">타입 분석이 끝났을 때의 문맥</param>
        public void ReportReadonlyCandidates(SymbolAnalysisContext context)
        {
            foreach (IFieldSymbol field in _candidates.Keys)
            {
                context.ReportDiagnostic(Diagnostic.Create(_rule, field.Locations[0], field.Name));
            }
        }

        private static bool IsCandidate(IFieldSymbol field)
        {
            if (field.DeclaredAccessibility != Accessibility.Private || field.IsImplicitlyDeclared)
            {
                return false;
            }

            if (field.IsReadOnly || field.IsConst || field.IsVolatile || field.IsFixedSizeBuffer)
            {
                return false;
            }

            return field.GetAttributes().Length == 0;
        }

        private static bool IsInitializingWrite(IFieldReferenceOperation reference, IFieldSymbol field, ISymbol containingSymbol)
        {
            IOperation ancestor = reference.Parent;
            while (ancestor != null)
            {
                if (ancestor is IAnonymousFunctionOperation || ancestor is ILocalFunctionOperation)
                {
                    return false;
                }

                ancestor = ancestor.Parent;
            }

            if (!(containingSymbol is IMethodSymbol method))
            {
                return false;
            }

            if (!SymbolEqualityComparer.Default.Equals(method.ContainingType, field.ContainingType))
            {
                return false;
            }

            if (field.IsStatic)
            {
                return method.MethodKind == MethodKind.StaticConstructor;
            }

            return method.MethodKind == MethodKind.Constructor && reference.Instance is IInstanceReferenceOperation;
        }
    }
}
