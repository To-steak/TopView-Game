#if UNITY_EDITOR
using TopView.Entities;
using UnityEngine;

namespace TopView.Core
{
    // 디버그 표시: 요소별 Gizmo를 부른다. 그리는 방법은 각 요소의 Runner.<요소>.cs에 두고, 근접 판정처럼 Gizmo뿐인 요소는 여기에 둔다
    public partial class Runner
    {
        private const int CIRCLE_GIZMO_SEGMENTS = 32;

        private void OnDrawGizmos()
        {
            if (_world == null)
            {
                return;
            }

            DrawPlayerGizmos();
            DrawEnemyGizmos();
            DrawMeleeGizmos();
            DrawProjectileGizmos();
            DrawLootGizmos();
        }

        // 지금 살아 있는 근접 판정마다 판정 원을 자홍으로 그린다. 플레이어와 적의 근접 공격이 모두 여기에 나온다.
        private void DrawMeleeGizmos()
        {
            MeleeEntity[] entities = _world.melees.entities;
            Gizmos.color = Color.magenta;
            for (int index = 0; index < entities.Length; index++)
            {
                if (entities[index].isAlive)
                {
                    DrawCircleGizmo(entities[index].position.current, entities[index].melee.radius);
                }
            }
        }

        // center 높이의 XZ 평면에 반지름 radius의 원을 지금 Gizmos 색으로 그린다.
        private static void DrawCircleGizmo(Vector3 center, float radius)
        {
            Vector3 previousPoint = center + new Vector3(radius, 0f, 0f);
            for (int segmentIndex = 1; segmentIndex <= CIRCLE_GIZMO_SEGMENTS; segmentIndex++)
            {
                float angle = segmentIndex * Mathf.PI * 2f / CIRCLE_GIZMO_SEGMENTS;
                Vector3 point = center + new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius);
                Gizmos.DrawLine(previousPoint, point);
                previousPoint = point;
            }
        }
    }
}
#endif
