using TopView.Authoring;
using UnityEditor;
using UnityEngine;

namespace TopView.Editor
{
    // 플레이어 감지기의 감지 범위를 씬 뷰에 그린다. 배치할 때 보이도록 플레이 중이 아니어도, 선택하지 않아도 그린다.
    public static class PlayerDetectorGizmo
    {
        [DrawGizmo(GizmoType.Selected | GizmoType.NonSelected)]
        private static void Draw(PlayerDetectorAuthoring authoring, GizmoType gizmoType)
        {
            Handles.color = Color.red;
            Handles.DrawWireDisc(authoring.transform.position, Vector3.up, authoring.range.detectionRadius);
        }
    }
}
