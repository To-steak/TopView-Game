using TopView.Components;
using UnityEditor;
using UnityEngine;

namespace TopView.Editor
{
    // EnemyAttackComponent 아래에 클립 변환 칸을 붙인다.
    // Body는 시작·끝 백분율과 클립 길이로 Start/End/Duration Ticks를 채운다. 투사체 공격은 클립 길이로 Duration Ticks를 채우고, 발마다의 시작 백분율은 Shots의 칸에서 채운다.
    // 공격 종류가 쓰지 않는 칸도 숨긴다. Body는 투사체와 Shots, 투사체 공격은 Body 판정과 Timing이다.
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
                return childName == nameof(EnemyAttackComponent.projectile)
                    || childName == nameof(EnemyAttackComponent.shots);
            }

            // 투사체 공격의 Duration Ticks는 아래 변환 칸에서 클립 칸과 함께 그린다
            return childName == nameof(EnemyAttackComponent.timing)
                || childName == nameof(EnemyAttackComponent.meleeShape);
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
            SerializedProperty timing = property.FindPropertyRelative(nameof(EnemyAttackComponent.timing));
            SerializedProperty durationTicks = timing.FindPropertyRelative(nameof(AttackTimingComponent.durationTicks));
            if (IsBody(property))
            {
                AnimationTickConverter.DrawRange(rect, timing.FindPropertyRelative(nameof(AttackTimingComponent.startTicks)), timing.FindPropertyRelative(nameof(AttackTimingComponent.endTicks)), durationTicks);
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
