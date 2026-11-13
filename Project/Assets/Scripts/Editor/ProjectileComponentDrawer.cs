using TopView.Components;
using UnityEditor;

namespace TopView.Editor
{
    // ProjectileComponent를 펼쳐 그리면서, 투사체 종류가 쓰지 않는 칸을 숨긴다. Straight는 비행 시간과 포물선 높이 칸, Grenade는 속도와 수명 칸이다.
    [CustomPropertyDrawer(typeof(ProjectileComponent))]
    public class ProjectileComponentDrawer : AnimationTicksDrawer
    {
        protected override bool IsLengthTicks(string propertyName)
        {
            return false;
        }

        protected override bool IsHidden(SerializedProperty property, string childName)
        {
            ProjectileKind kind = (ProjectileKind)property.FindPropertyRelative(nameof(ProjectileComponent.kind)).intValue;
            if (kind == ProjectileKind.Grenade)
            {
                return childName == nameof(ProjectileComponent.speed)
                    || childName == nameof(ProjectileComponent.lifetimeTicks);
            }

            return childName == nameof(ProjectileComponent.flightTicks)
                || childName == nameof(ProjectileComponent.arcHeight);
        }
    }
}
