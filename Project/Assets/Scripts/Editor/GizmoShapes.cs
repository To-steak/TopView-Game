using UnityEngine;

namespace TopView.Editor
{
    // Gizmo 클래스들이 함께 쓰는 도형 그리기.
    public static class GizmoShapes
    {
        private const int CIRCLE_SEGMENTS = 32;

        /// <summary>
        /// center 높이의 XZ 평면에 원을 지금 Gizmos 색으로 그린다.
        /// </summary>
        /// <param name="center">원의 중심(월드)</param>
        /// <param name="radius">원의 반지름(유닛)</param>
        public static void DrawCircle(Vector3 center, float radius)
        {
            Vector3 previousPoint = center + new Vector3(radius, 0f, 0f);
            for (int segmentIndex = 1; segmentIndex <= CIRCLE_SEGMENTS; segmentIndex++)
            {
                float angle = segmentIndex * Mathf.PI * 2f / CIRCLE_SEGMENTS;
                Vector3 point = center + new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius);
                Gizmos.DrawLine(previousPoint, point);
                previousPoint = point;
            }
        }

        /// <summary>
        /// center 높이의 XZ 평면에 월드 축에 맞춘 사각형을 지금 Gizmos 색으로 그린다.
        /// </summary>
        /// <param name="center">사각형의 중심(월드)</param>
        /// <param name="size">사각형의 전체 크기(유닛). x는 월드 X, y는 월드 Z 길이다</param>
        public static void DrawRectangle(Vector3 center, Vector2 size)
        {
            Gizmos.DrawWireCube(center, new Vector3(size.x, 0f, size.y));
        }
    }
}
