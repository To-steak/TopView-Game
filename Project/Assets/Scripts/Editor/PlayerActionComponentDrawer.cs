using TopView.Components;
using UnityEditor;
using UnityEngine;

namespace TopView.Editor
{
    // PlayerActionComponent의 상태 길이 칸마다 클립 칸을 붙인다. 클립을 넣으면 클립 길이 전체가 틱으로 채워진다.
    // 무기 교체는 아래의 클립 구간 변환 칸에서 시작 백분율로 Swap Start Ticks를, 클립 길이로 Swap Ticks를 채운다.
    [CustomPropertyDrawer(typeof(PlayerActionComponent))]
    public class PlayerActionComponentDrawer : AnimationTicksDrawer
    {
        private const string SWAP_RANGE_LABEL = "Swap Clip";

        protected override bool IsLengthTicks(string propertyName)
        {
            return propertyName == nameof(PlayerActionComponent.dodgeTicks)
                || propertyName == nameof(PlayerActionComponent.landTicks)
                || propertyName == nameof(PlayerActionComponent.reloadTicks)
                || propertyName == nameof(PlayerActionComponent.dieTicks);
        }

        protected override float GetExtraHeight(SerializedProperty property)
        {
            return EditorGUIUtility.standardVerticalSpacing + AnimationTickConverter.LineHeight + AnimationTickConverter.RangeHeight;
        }

        protected override void DrawExtra(Rect rect, SerializedProperty property)
        {
            rect.y += EditorGUIUtility.standardVerticalSpacing;
            Rect labelRect = new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight);
            EditorGUI.LabelField(labelRect, SWAP_RANGE_LABEL, EditorStyles.boldLabel);

            Rect rangeRect = new Rect(rect.x, rect.y + AnimationTickConverter.LineHeight, rect.width, AnimationTickConverter.RangeHeight);
            SerializedProperty startTicks = property.FindPropertyRelative(nameof(PlayerActionComponent.swapStartTicks));
            SerializedProperty durationTicks = property.FindPropertyRelative(nameof(PlayerActionComponent.swapTicks));
            AnimationTickConverter.DrawRange(rangeRect, startTicks, null, durationTicks);
        }
    }
}
