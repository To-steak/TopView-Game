using TopView.Components;
using TopView.Definitions;
using UnityEditor;

namespace TopView.Editor
{
    // EnemyAttackEntry를 펼쳐 그리면서, Body 공격이면 쓰지 않는 프리팹 칸을 숨긴다.
    [CustomPropertyDrawer(typeof(EnemyAttackEntry))]
    public class EnemyAttackEntryDrawer : AnimationTicksDrawer
    {
        protected override bool IsLengthTicks(string propertyName)
        {
            return false;
        }

        protected override bool IsHidden(SerializedProperty property, string childName)
        {
            if (childName != nameof(EnemyAttackEntry.projectilePrefab) && childName != nameof(EnemyAttackEntry.hitEffectPrefab))
            {
                return false;
            }

            SerializedProperty kind = property.FindPropertyRelative($"{nameof(EnemyAttackEntry.attack)}.{nameof(EnemyAttackComponent.kind)}");
            return (EnemyAttackKind)kind.intValue == EnemyAttackKind.Body;
        }
    }
}
