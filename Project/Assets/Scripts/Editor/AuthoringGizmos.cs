using TopView.Authoring;
using TopView.Components;
using UnityEditor;
using UnityEngine;

namespace TopView.Editor
{
    // 씬에 배치한 Authoring의 값을 씬 뷰에 그린다. 배치할 때 보이도록 플레이 중이 아니어도, 선택하지 않아도 그린다.
    public static class AuthoringGizmos
    {
        private const float SPAWNER_POSITION_RADIUS = 0.3f;
        private const float SPAWNER_DIRECTION_LENGTH = 1f;
        private const float SPAWNER_LABEL_HEIGHT = 0.5f;

        // 플레이어 감지기의 감지 범위(XZ 원 또는 사각형)와 연결된 스포너까지의 선을 빨간색으로 그린다.
        [DrawGizmo(GizmoType.Selected | GizmoType.NonSelected)]
        private static void DrawPlayerDetector(PlayerDetectorAuthoring authoring, GizmoType gizmoType)
        {
            Vector3 center = authoring.transform.position;

            Gizmos.color = Color.red;
            switch (authoring.range.shape)
            {
                case PlayerDetectorShapeKind.Circle:
                    GizmoShapes.DrawCircle(center, authoring.range.detectionRadius);
                    break;

                case PlayerDetectorShapeKind.Rectangle:
                    GizmoShapes.DrawRectangle(center, authoring.range.detectionSize);
                    break;
            }
            DrawSpawnerLinks(authoring, center);
        }

        // 적 스포너의 위치와 방향을 청록색으로 그리고, 설정값을 글자로 띄운다.
        [DrawGizmo(GizmoType.Selected | GizmoType.NonSelected)]
        private static void DrawEnemySpawner(EnemySpawnerAuthoring authoring, GizmoType gizmoType)
        {
            Transform spawnerTransform = authoring.transform;
            Vector3 position = spawnerTransform.position;

            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(position, SPAWNER_POSITION_RADIUS);
            Gizmos.DrawLine(position, position + spawnerTransform.forward * SPAWNER_DIRECTION_LENGTH);

            string definitionName = authoring.definition != null ? authoring.definition.name : "Definition 없음";
            string label = $"{definitionName}\n주기 {authoring.rule.spawnIntervalTicks}틱 / 최대 {authoring.rule.maxSpawnCount}마리";
            Handles.Label(position + Vector3.up * SPAWNER_LABEL_HEIGHT, label);
        }

        // 연결된 스포너마다 감지기에서 스포너까지 지금 Gizmos 색으로 선을 그린다. 빈 슬롯은 건너뛴다.
        private static void DrawSpawnerLinks(PlayerDetectorAuthoring authoring, Vector3 center)
        {
            if (authoring.spawners == null)
            {
                return;
            }

            for (int linkIndex = 0; linkIndex < authoring.spawners.Length; linkIndex++)
            {
                EnemySpawnerAuthoring spawner = authoring.spawners[linkIndex];
                if (spawner != null)
                {
                    Gizmos.DrawLine(center, spawner.transform.position);
                }
            }
        }
    }
}
