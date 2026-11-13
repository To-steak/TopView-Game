namespace TopView.Analyzers.Tests
{
    /// <summary>
    /// 테스트가 쓰는 UnityEngine 타입의 최소 대역. 실제 UnityEngine.dll 없이 분석기가 Unity 타입을 알아보는지 시험한다.
    /// 별도 어셈블리(UnityEngine)로 컴파일해 참조하므로 분석기의 검사 대상(TopView 어셈블리)이 아니다.
    /// </summary>
    public static class UnityStubSource
    {
        public const string ASSEMBLY_NAME = "UnityEngine";

        public const string TEXT = @"
namespace UnityEngine
{
    public class Object { }
    public class Component : Object { }
    public class Behaviour : Component { }
    public class MonoBehaviour : Behaviour { }
    public class ScriptableObject : Object { }
    public enum KeyCode { A }
    public static class Input
    {
        public static bool GetKey(KeyCode key) { return false; }
    }
    public struct Vector3
    {
        public float x;
        public float y;
        public float z;
    }
    [System.AttributeUsage(System.AttributeTargets.Field)]
    public sealed class SerializeField : System.Attribute { }
    public sealed class HeaderAttribute : System.Attribute
    {
        public HeaderAttribute(string header) { }
    }
    public sealed class ContextMenu : System.Attribute
    {
        public ContextMenu(string itemName) { }
    }
}
";
    }
}
