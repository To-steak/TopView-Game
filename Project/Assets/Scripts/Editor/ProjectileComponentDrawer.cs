using TopView.Components;
using UnityEditor;

namespace TopView.Editor
{
    // ProjectileComponent를 펼쳐 그리면서, 투사체 종류가 쓰지 않는 칸을 숨긴다. Straight는 수류탄 칸(환경 레이어 제외), Grenade는 속도와 수명 칸이다.
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

            return childName == nameof(ProjectileComponent.maxThrowDistance)
                || childName == nameof(ProjectileComponent.upwardRatio)
                || childName == nameof(ProjectileComponent.gravity)
                || childName == nameof(ProjectileComponent.fuseTicks)
                || childName == nameof(ProjectileComponent.explosionRadius);
        }
    }
}
