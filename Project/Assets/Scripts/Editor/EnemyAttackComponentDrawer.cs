using TopView.Components;
using UnityEditor;
using UnityEngine;

namespace TopView.Editor
{
    // EnemyAttackComponent 아래에 클립 변환 칸을 붙인다.
    // Body는 시작·끝 백분율과 클립 길이로 근접 공격의 Start/End Ticks와 Duration Ticks를 채운다. 투사체 공격은 클립 길이로 Duration Ticks를 채우고, 발마다의 시작 백분율은 Shots의 칸에서 채운다.
    // 공격 종류가 쓰지 않는 칸도 숨긴다. Body는 직선 투사체 공격, 투사체 공격은 근접 공격이다.
    [CustomPropertyDrawer(typeof(EnemyAttackComponent))]
    public class EnemyAttackComponentDrawer : AnimationTicksDrawer
    {
        protected override bool IsLengthTicks(string propertyName)
        {
            return false;
        }

        protected override bool IsHidden(SerializedProperty property, string childName)
        {
            if (IsBody(property))
            {
                return childName == nameof(EnemyAttackComponent.straight);
            }

            // 투사체 공격의 Duration Ticks는 아래 변환 칸에서 클립 칸과 함께 그린다
            return childName == nameof(EnemyAttackComponent.melee)
                || childName == nameof(EnemyAttackComponent.durationTicks);
        }

        protected override float GetExtraHeight(SerializedProperty property)
        {
            float extraHeight = IsBody(property) ? AnimationTickConverter.RangeHeight : AnimationTickConverter.LineHeight;
            return EditorGUIUtility.standardVerticalSpacing + extraHeight;
        }

        protected override void DrawExtra(Rect rect, SerializedProperty property)
        {
            rect.y += EditorGUIUtility.standardVerticalSpacing;
            rect.height -= EditorGUIUtility.standardVerticalSpacing;
            SerializedProperty durationTicks = property.FindPropertyRelative(nameof(EnemyAttackComponent.durationTicks));
            if (IsBody(property))
            {
                SerializedProperty melee = property.FindPropertyRelative(nameof(EnemyAttackComponent.melee));
                AnimationTickConverter.DrawRange(rect, melee.FindPropertyRelative(nameof(MeleeAttackComponent.startTicks)), melee.FindPropertyRelative(nameof(MeleeAttackComponent.endTicks)), durationTicks);
            }
            else
            {
                AnimationTickConverter.DrawLengthTicks(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), durationTicks);
            }
        }

        private static bool IsBody(SerializedProperty property)
        {
            return (EnemyAttackKind)property.FindPropertyRelative(nameof(EnemyAttackComponent.kind)).intValue == EnemyAttackKind.Body;
        }
    }
}
