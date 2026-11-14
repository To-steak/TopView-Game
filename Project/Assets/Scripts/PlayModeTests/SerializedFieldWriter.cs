using System.Reflection;
using NUnit.Framework;

namespace TopView.PlayModeTests
{
    // 표시 테스트가 만든 View와 Presenter에, 인스펙터에서 받는 private 직렬화 필드를 채운다.
    public static class SerializedFieldWriter
    {
        /// <summary>
        /// 대상의 private 인스턴스 필드에 값을 넣는다. 필드가 없으면 테스트를 실패시킨다.
        /// </summary>
        /// <param name="target">값을 넣을 객체</param>
        /// <param name="fieldName">필드 이름(예: _healthText)</param>
        /// <param name="value">넣을 값</param>
        public static void Write(object target, string fieldName, object value)
        {
            FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(field, $"{target.GetType().Name}에 {fieldName} 필드가 없다. 테스트가 쓰는 필드 이름을 코드에 맞춘다.");
            field.SetValue(target, value);
        }
    }
}
