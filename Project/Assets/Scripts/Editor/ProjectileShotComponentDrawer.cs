using TopView.Components;
using TopView.Definitions;
using UnityEditor;
using UnityEngine;

namespace TopView.Editor
{
    // ProjectileShotComponent 아래에 클립 구간 변환 칸과 발사 위치 굽기 칸을 붙인다.
    // 시작 백분율과 클립 길이로 Start Ticks를 채우고, 같은 클립을 그 시점으로 샘플링해 Spawn Offset을 채운다. 무기는 무기 모델의 muzzle, 적은 고른 Muzzle의 위치를 쓴다.
    [CustomPropertyDrawer(typeof(ProjectileShotComponent))]
    public class ProjectileShotComponentDrawer : AnimationTicksDrawer
    {
        protected override bool IsLengthTicks(string propertyName)
        {
            return false;
        }

        protected override float GetExtraHeight(SerializedProperty property)
        {
            return EditorGUIUtility.standardVerticalSpacing + AnimationTickConverter.RangeHeight + GetBakeHeight(property);
        }

        protected override void DrawExtra(Rect rect, SerializedProperty property)
        {
            rect.y += EditorGUIUtility.standardVerticalSpacing;
            SerializedProperty startTicks = property.FindPropertyRelative(nameof(ProjectileShotComponent.startTicks));
            Rect rangeRect = new Rect(rect.x, rect.y, rect.width, AnimationTickConverter.RangeHeight);
            AnimationTickConverter.DrawRange(rangeRect, startTicks, null, null);

            Rect bakeRect = new Rect(rect.x, rangeRect.yMax, rect.width, GetBakeHeight(property));
            AnimationClip clip = AnimationTickConverter.GetRangeClip(startTicks);
            if (IsWeaponShot(property))
            {
                MuzzleOffsetBaker.Draw(bakeRect, property, clip);
            }
            else
            {
                EnemySpawnOffsetBaker.Draw(bakeRect, property, clip);
            }
        }

        private static float GetBakeHeight(SerializedProperty property)
        {
            return IsWeaponShot(property) ? MuzzleOffsetBaker.Height : EnemySpawnOffsetBaker.Height;
        }

        private static bool IsWeaponShot(SerializedProperty property)
        {
            return property.serializedObject.targetObject is WeaponDefinition;
        }
    }
}
