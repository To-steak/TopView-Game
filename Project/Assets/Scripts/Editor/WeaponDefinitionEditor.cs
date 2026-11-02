using TopView.Components;
using TopView.Definitions;
using UnityEditor;

namespace TopView.Editor
{
    // 인스펙터와 Definition Editor 창에서 WeaponDefinition을 그린다. 무기 종류(kind)에 따라 그 종류가 쓰는 칸만 보인다.
    // 숨긴 칸의 값은 지우지 않고 그대로 둔다.
    [CustomEditor(typeof(WeaponDefinition))]
    public class WeaponDefinitionEditor : UnityEditor.Editor
    {
        private const string SCRIPT_PROPERTY_NAME = "m_Script";

        /// <summary>
        /// 공통 칸을 그리고, 무기 종류에 맞는 칸만 이어서 그린다.
        /// </summary>
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty(SCRIPT_PROPERTY_NAME));
            }

            SerializedProperty weapon = serializedObject.FindProperty(nameof(WeaponDefinition.weapon));
            SerializedProperty kindProperty = weapon.FindPropertyRelative(nameof(WeaponComponent.kind));
            WeaponKind kind = (WeaponKind)kindProperty.intValue;
            bool usesProjectile = kind != WeaponKind.Melee;

            EditorGUILayout.LabelField(weapon.displayName, EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(kindProperty);
            DrawRelative(weapon, nameof(WeaponComponent.damage));
            DrawAttackTicks(weapon, kind);
            if (kind != WeaponKind.Throw)
            {
                DrawRelative(weapon, nameof(WeaponComponent.attackIntervalTicks));
            }

            if (kind == WeaponKind.Ranged)
            {
                DrawRelative(weapon, nameof(WeaponComponent.maxAmmo));
                DrawRelative(weapon, nameof(WeaponComponent.currentAmmo));
            }

            if (usesProjectile)
            {
                DrawRelative(weapon, nameof(WeaponComponent.reserveAmmo));
                DrawRelative(weapon, nameof(WeaponComponent.projectile));
            }
            else
            {
                DrawRelative(weapon, nameof(WeaponComponent.meleeRadius));
                DrawRelative(weapon, nameof(WeaponComponent.meleeOffset));
            }
            EditorGUI.indentLevel--;

            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(WeaponDefinition.modelPrefab)));
            if (usesProjectile)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(WeaponDefinition.projectilePrefab)));
                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(WeaponDefinition.hitEffectPrefab)));
            }

            serializedObject.ApplyModifiedProperties();
        }

        // 공격 시간 칸과 클립 구간 변환 칸을 그린다. Melee는 Start/End/Duration, Throw는 Start/Duration, Ranged는 발사하면 끝나므로 Start만 쓴다.
        private static void DrawAttackTicks(SerializedProperty weapon, WeaponKind kind)
        {
            SerializedProperty startTicks = weapon.FindPropertyRelative(nameof(WeaponComponent.attackStartTicks));
            EditorGUILayout.PropertyField(startTicks);
            if (kind == WeaponKind.Ranged)
            {
                AnimationTickConverter.DrawRange(EditorGUILayout.GetControlRect(false, AnimationTickConverter.RangeHeight), startTicks, null, null);
                return;
            }

            SerializedProperty durationTicks = weapon.FindPropertyRelative(nameof(WeaponComponent.attackDurationTicks));
            if (kind == WeaponKind.Throw)
            {
                EditorGUILayout.PropertyField(durationTicks);
                AnimationTickConverter.DrawRange(EditorGUILayout.GetControlRect(false, AnimationTickConverter.RangeHeight), startTicks, null, durationTicks);
                return;
            }

            SerializedProperty endTicks = weapon.FindPropertyRelative(nameof(WeaponComponent.attackEndTicks));
            EditorGUILayout.PropertyField(endTicks);
            EditorGUILayout.PropertyField(durationTicks);
            AnimationTickConverter.DrawRange(EditorGUILayout.GetControlRect(false, AnimationTickConverter.RangeHeight), startTicks, endTicks, durationTicks);
        }

        // 부모 칸 안의 자식 칸 하나를 자식까지 펼쳐 그린다.
        private static void DrawRelative(SerializedProperty parent, string propertyName)
        {
            EditorGUILayout.PropertyField(parent.FindPropertyRelative(propertyName), true);
        }
    }
}
