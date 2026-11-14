using System;
using System.Reflection;
using NUnit.Framework;

namespace TopView.Tests.PlayMode
{
    // 테스트가 게임 코드의 private 필드(World·App의 데이터, 인스펙터에서 받는 직렬화 필드)를 리플렉션으로 읽고 쓴다.
    // 게임 코드에 테스트용 통로를 두지 않기 위해서다. 필드를 찾지 못하면 어떤 타입의 어떤 필드인지 알리며 테스트를 실패시킨다.
    public static class PrivateFieldAccess
    {
        private const BindingFlags INSTANCE_FIELD = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;

        /// <summary>
        /// 대상의 인스턴스 필드 값을 읽는다. struct 필드는 복사본이 돌아오므로, 고친 뒤 Write로 되돌려 넣는다.
        /// </summary>
        /// <typeparam name="T">필드의 타입</typeparam>
        /// <param name="target">값을 읽을 객체</param>
        /// <param name="fieldName">필드 이름(예: _progress)</param>
        /// <returns>필드의 값</returns>
        public static T Read<T>(object target, string fieldName)
        {
            object value = FindField(target, fieldName).GetValue(target);
            Assert.IsTrue(value == null || value is T, $"{target.GetType().Name}.{fieldName}의 타입이 {typeof(T).Name}이 아니다.");
            return (T)value;
        }

        /// <summary>
        /// 대상의 인스턴스 필드에 값을 넣는다.
        /// </summary>
        /// <param name="target">값을 넣을 객체</param>
        /// <param name="fieldName">필드 이름(예: _healthText)</param>
        /// <param name="value">넣을 값</param>
        public static void Write(object target, string fieldName, object value)
        {
            FindField(target, fieldName).SetValue(target, value);
        }

        // 상속한 타입의 private 필드도 찾도록 기반 타입까지 올라가며 찾는다.
        private static FieldInfo FindField(object target, string fieldName)
        {
            for (Type type = target.GetType(); type != null; type = type.BaseType)
            {
                FieldInfo field = type.GetField(fieldName, INSTANCE_FIELD | BindingFlags.DeclaredOnly);
                if (field != null)
                {
                    return field;
                }
            }

            Assert.Fail($"{target.GetType().Name}에 {fieldName} 필드가 없다. 게임 코드의 필드 이름이 바뀌었으면 테스트를 맞춘다.");
            return null;
        }
    }
}
