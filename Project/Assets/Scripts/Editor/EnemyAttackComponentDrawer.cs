using TopView.Components;
using UnityEditor;
using UnityEngine;

namespace TopView.Editor
{
    // EnemyAttackComponent 아래에 클립 구간 변환 칸을 붙인다.
    // Body는 시작·끝 백분율과 클립 길이로 Start/End/Duration Ticks를, 투사체 공격은 시작 백분율로 Start Ticks만 채우고 End/Duration 칸을 숨긴다.
    [CustomPropertyDrawer(typeof(EnemyAttackComponent))]
    public class EnemyAttackComponentDrawer : AnimationTicksDrawer
    {
        protected override bool IsLengthTicks(string propertyName)
        {
            return false;
        }

        protected override bool IsHidden(SerializedProperty property, string childName)
        {
            return !IsBody(property)
                && (childName == nameof(EnemyAttackComponent.endTicks) || childName == nameof(EnemyAttackComponent.durationTicks));
        }

        protected override float GetExtraHeight(SerializedProperty property)
        {
            return EditorGUIUtility.standardVerticalSpacing + AnimationTickConverter.RangeHeight;
        }

        protected override void DrawExtra(Rect rect, SerializedProperty property)
        {
            rect.y += EditorGUIUtility.standardVerticalSpacing;
            rect.height -= EditorGUIUtility.standardVerticalSpacing;
            SerializedProperty startTicks = property.FindPropertyRelative(nameof(EnemyAttackComponent.startTicks));
            if (IsBody(property))
            {
                AnimationTickConverter.DrawRange(rect, startTicks, property.FindPropertyRelative(nameof(EnemyAttackComponent.endTicks)), property.FindPropertyRelative(nameof(EnemyAttackComponent.durationTicks)));
            }
            else
            {
                AnimationTickConverter.DrawRange(rect, startTicks, null, null);
            }
        }

        private static bool IsBody(SerializedProperty property)
        {
            return (EnemyAttackKind)property.FindPropertyRelative(nameof(EnemyAttackComponent.kind)).intValue == EnemyAttackKind.Body;
        }
    }
}
