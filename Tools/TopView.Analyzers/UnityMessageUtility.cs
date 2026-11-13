using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace TopView.Analyzers
{
    /// <summary>
    /// Unity가 이름으로 찾아 호출하는 메시지 메서드(<c>Awake</c>, <c>OnEnable</c> 등)를 알아본다.
    /// 코드에서 호출하지 않아도 쓰이는 메서드라서, 안 쓰는 멤버 검사에서 뺀다.
    /// </summary>
    public static class UnityMessageUtility
    {
        private static readonly HashSet<string> _messageNames = new HashSet<string>
        {
            "Awake", "Start", "Update", "FixedUpdate", "LateUpdate", "Reset",
            "OnEnable", "OnDisable", "OnDestroy", "OnValidate", "OnGUI",
            "OnDrawGizmos", "OnDrawGizmosSelected",
            "OnApplicationQuit", "OnApplicationPause", "OnApplicationFocus",
            "OnBecameVisible", "OnBecameInvisible",
            "OnTriggerEnter", "OnTriggerExit", "OnTriggerStay",
            "OnTriggerEnter2D", "OnTriggerExit2D", "OnTriggerStay2D",
            "OnCollisionEnter", "OnCollisionExit", "OnCollisionStay",
            "OnCollisionEnter2D", "OnCollisionExit2D", "OnCollisionStay2D",
            "OnControllerColliderHit", "OnJointBreak", "OnJointBreak2D",
            "OnParticleCollision", "OnParticleTrigger", "OnParticleSystemStopped",
            "OnMouseDown", "OnMouseUp", "OnMouseUpAsButton", "OnMouseEnter", "OnMouseExit", "OnMouseOver", "OnMouseDrag",
            "OnAnimatorMove", "OnAnimatorIK", "OnDidApplyAnimationProperties",
            "OnPreCull", "OnPreRender", "OnPostRender", "OnRenderObject", "OnWillRenderObject", "OnRenderImage",
            "OnAudioFilterRead",
            "OnTransformParentChanged", "OnTransformChildrenChanged", "OnBeforeTransformParentChanged",
            "OnRectTransformDimensionsChange", "OnCanvasGroupChanged",
            "OnSceneGUI", "OnInspectorUpdate", "OnSelectionChange", "OnFocus", "OnLostFocus",
            "OnHierarchyChange", "OnProjectChange", "CreateGUI",
        };

        private const string UNITY_OBJECT_TYPE_NAME = "UnityEngine.Object";

        /// <summary>
        /// 메서드가 Unity 메시지인지 확인한다.
        /// </summary>
        /// <param name="method">검사할 메서드</param>
        /// <returns>UnityEngine.Object를 상속한 타입에 선언된 메시지 이름의 메서드면 true</returns>
        public static bool IsUnityMessage(IMethodSymbol method)
        {
            if (!_messageNames.Contains(method.Name))
            {
                return false;
            }

            return InheritsUnityObject(method.ContainingType);
        }

        private static bool InheritsUnityObject(INamedTypeSymbol type)
        {
            INamedTypeSymbol current = type.BaseType;
            while (current != null)
            {
                if (current.ToDisplayString() == UNITY_OBJECT_TYPE_NAME)
                {
                    return true;
                }

                current = current.BaseType;
            }

            return false;
        }
    }
}
