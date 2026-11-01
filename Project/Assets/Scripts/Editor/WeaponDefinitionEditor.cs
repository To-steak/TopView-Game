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
            DrawRelative(weapon, nameof(WeaponComponent.attackCommitTicks));
            DrawRelative(weapon, nameof(WeaponComponent.attackDurationTicks));

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

        // 부모 칸 안의 자식 칸 하나를 자식까지 펼쳐 그린다.
        private static void DrawRelative(SerializedProperty parent, string propertyName)
        {
            EditorGUILayout.PropertyField(parent.FindPropertyRelative(propertyName), true);
        }
    }
}
