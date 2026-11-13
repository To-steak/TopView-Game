using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

namespace TopView.Analyzers
{
    /// <summary>
    /// 필드·매개변수 참조 연산이 그 값을 바꾸는 쓰기인지 판정한다.
    /// </summary>
    public static class WriteDetectionUtility
    {
        /// <summary>
        /// 참조가 값을 바꾸는지 확인한다.
        /// 대입·복합 대입·증감·ref/out 전달·주소 연산·ref 지역 변수가 쓰기다.
        /// 값을 바꿀 수 있는 struct는 그 멤버에 쓰거나, readonly가 아닌 메서드를 부르는 것도 쓰기다.
        /// </summary>
        /// <param name="reference">필드·매개변수·멤버 참조 연산</param>
        /// <param name="valueType">참조가 가리키는 값의 타입</param>
        /// <returns>값을 바꾸면 true</returns>
        public static bool IsWrite(IOperation reference, ITypeSymbol valueType)
        {
            IOperation current = reference;
            IOperation parent = reference.Parent;
            while (parent is ITupleOperation)
            {
                current = parent;
                parent = parent.Parent;
            }

            switch (parent)
            {
                case IAssignmentOperation assignment when assignment.Target == current:
                    return true;
                case IIncrementOrDecrementOperation _:
                    return true;
                case IAddressOfOperation _:
                    return true;
                case IArgumentOperation argument when argument.Parameter != null:
                    if (argument.Parameter.RefKind == RefKind.Ref || argument.Parameter.RefKind == RefKind.Out)
                    {
                        return true;
                    }

                    break;
            }

            if (reference.Syntax.Parent is RefExpressionSyntax)
            {
                return true;
            }

            if (!IsMutableStruct(valueType))
            {
                return false;
            }

            switch (reference.Parent)
            {
                case IFieldReferenceOperation fieldReference when fieldReference.Instance == reference:
                    return IsWrite(fieldReference, fieldReference.Field.Type);
                case IPropertyReferenceOperation propertyReference when propertyReference.Instance == reference:
                    return IsWrite(propertyReference, propertyReference.Property.Type);
                case IInvocationOperation invocation when invocation.Instance == reference:
                    return !invocation.TargetMethod.IsReadOnly;
                default:
                    return false;
            }
        }

        /// <summary>
        /// 값을 바꿀 수 있는 struct인지 확인한다. 기본 타입, enum, readonly struct는 아니다.
        /// </summary>
        /// <param name="type">검사할 타입</param>
        /// <returns>값을 바꿀 수 있는 struct면 true</returns>
        public static bool IsMutableStruct(ITypeSymbol type)
        {
            if (type == null || !type.IsValueType || type.SpecialType != SpecialType.None || type.TypeKind == TypeKind.Enum)
            {
                return false;
            }

            return !type.IsReadOnly;
        }
    }
}
