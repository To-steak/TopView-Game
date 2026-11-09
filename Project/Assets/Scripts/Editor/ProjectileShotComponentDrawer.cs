using TopView.Components;
using UnityEditor;
using UnityEngine;

namespace TopView.Editor
{
    // ProjectileShotComponent 아래에 클립 구간 변환 칸과 발사 위치 굽기 칸을 붙인다.
    // 시작 백분율과 클립 길이로 Start Ticks를 채우고, 같은 클립을 그 시점으로 샘플링해 고른 Muzzle의 위치로 Spawn Offset을 채운다.
    [CustomPropertyDrawer(typeof(ProjectileShotComponent))]
    public class ProjectileShotComponentDrawer : AnimationTicksDrawer
    {
        protected override bool IsLengthTicks(string propertyName)
        {
            return false;
        }

        protected override float GetExtraHeight(SerializedProperty property)
        {
            return EditorGUIUtility.standardVerticalSpacing + AnimationTickConverter.RangeHeight + EnemySpawnOffsetBaker.Height;
        }

        protected override void DrawExtra(Rect rect, SerializedProperty property)
        {
            rect.y += EditorGUIUtility.standardVerticalSpacing;
            SerializedProperty startTicks = property.FindPropertyRelative(nameof(ProjectileShotComponent.startTicks));
            Rect rangeRect = new Rect(rect.x, rect.y, rect.width, AnimationTickConverter.RangeHeight);
            AnimationTickConverter.DrawRange(rangeRect, startTicks, null, null);

            Rect bakeRect = new Rect(rect.x, rangeRect.yMax, rect.width, EnemySpawnOffsetBaker.Height);
            EnemySpawnOffsetBaker.Draw(bakeRect, property, AnimationTickConverter.GetRangeClip(startTicks));
        }
    }
}
