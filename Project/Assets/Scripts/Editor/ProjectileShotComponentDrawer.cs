using TopView.Components;
using UnityEditor;
using UnityEngine;

namespace TopView.Editor
{
    // ProjectileShotComponent 아래에 클립 구간 변환 칸을 붙인다. 시작 백분율과 클립 길이로 Start Ticks를 채운다.
    [CustomPropertyDrawer(typeof(ProjectileShotComponent))]
    public class ProjectileShotComponentDrawer : AnimationTicksDrawer
    {
        protected override bool IsLengthTicks(string propertyName)
        {
            return false;
        }

        protected override float GetExtraHeight(SerializedProperty property)
        {
            return EditorGUIUtility.standardVerticalSpacing + AnimationTickConverter.RangeHeight;
        }

        protected override void DrawExtra(Rect rect, SerializedProperty property)
        {
            rect.y += EditorGUIUtility.standardVerticalSpacing;
            rect.height -= EditorGUIUtility.standardVerticalSpacing;
            AnimationTickConverter.DrawRange(rect, property.FindPropertyRelative(nameof(ProjectileShotComponent.startTicks)), null, null);
        }
    }
}
