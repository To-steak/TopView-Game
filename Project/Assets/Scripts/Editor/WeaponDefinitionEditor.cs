using TopView.Components;
using TopView.Definitions;
using UnityEditor;
using UnityEngine;

namespace TopView.Editor
{
    // 인스펙터와 Definition Editor 창에서 WeaponDefinition을 그린다. 무기 종류(kind)에 따라 그 종류가 쓰는 칸만 보인다.
    // 숨긴 칸의 값은 지우지 않고 그대로 둔다.
    [CustomEditor(typeof(WeaponDefinition))]
    public class WeaponDefinitionEditor : UnityEditor.Editor
    {
        private const string SCRIPT_PROPERTY_NAME = "m_Script";
        // 묶음 안으로 들어가 이름이 짧아진 탄약 칸에 붙이는 이름
        private const string MAX_AMMO_LABEL = "Max Ammo";
        private const string STARTING_CURRENT_AMMO_LABEL = "Starting Current Ammo";
        private const string STARTING_RESERVE_AMMO_LABEL = "Starting Reserve Ammo";

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

            SerializedProperty ammo = weapon.FindPropertyRelative(nameof(WeaponComponent.ammo));
            SerializedProperty upgrade = weapon.FindPropertyRelative(nameof(WeaponComponent.upgrade));

            EditorGUILayout.LabelField(weapon.displayName, EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(kindProperty);
            DrawRelative(weapon, nameof(WeaponComponent.damage));
            DrawAttackTicks(weapon, kind);
            if (kind != WeaponKind.Throw)
            {
                DrawRelative(weapon, nameof(WeaponComponent.attackIntervalTicks));
                DrawRelative(upgrade, nameof(WeaponUpgradeRuleComponent.attackIntervalRatioPerLevel));
            }

            if (kind == WeaponKind.Ranged)
            {
                DrawRelative(ammo, nameof(AmmoComponent.max), MAX_AMMO_LABEL);
                DrawRelative(upgrade, nameof(WeaponUpgradeRuleComponent.maxAmmoRatioPerLevel));
                DrawRelative(ammo, nameof(AmmoComponent.startingCurrent), STARTING_CURRENT_AMMO_LABEL);
            }

            if (usesProjectile)
            {
                DrawRelative(ammo, nameof(AmmoComponent.startingReserve), STARTING_RESERVE_AMMO_LABEL);
                DrawRelative(weapon, nameof(WeaponComponent.projectile));
                if (kind == WeaponKind.Throw)
                {
                    DrawRelative(weapon, nameof(WeaponComponent.throwing));
                }
                DrawRelative(upgrade, nameof(WeaponUpgradeRuleComponent.explosionRadiusIncreasePerLevel));
            }
            else
            {
                DrawRelative(weapon, nameof(WeaponComponent.meleeShape));
                DrawRelative(upgrade, nameof(WeaponUpgradeRuleComponent.meleeRangeRatioPerLevel));
            }
            // 조준 평면의 높이에 모든 종류가 쓴다
            SerializedProperty muzzleOffset = weapon.FindPropertyRelative(nameof(WeaponComponent.muzzleOffset));
            EditorGUILayout.PropertyField(muzzleOffset);
            SerializedProperty timing = weapon.FindPropertyRelative(nameof(WeaponComponent.timing));
            MuzzleOffsetBaker.Draw((WeaponDefinition)target, muzzleOffset, timing.FindPropertyRelative(nameof(AttackTimingComponent.startTicks)).intValue);
            EditorGUI.indentLevel--;

            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(WeaponDefinition.modelPrefab)));
            if (usesProjectile)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(WeaponDefinition.projectilePrefab)));
                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(WeaponDefinition.hitEffectPrefab)));
            }
            else
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(WeaponDefinition.impactEffectPrefab)));
            }

            serializedObject.ApplyModifiedProperties();
        }

        // 공격 시간 칸과 클립 구간 변환 칸을 그린다. Melee는 Start/End/Duration, Throw는 Start/Duration, Ranged는 발사하면 끝나므로 Start만 쓴다.
        private static void DrawAttackTicks(SerializedProperty weapon, WeaponKind kind)
        {
            SerializedProperty timing = weapon.FindPropertyRelative(nameof(WeaponComponent.timing));
            SerializedProperty startTicks = timing.FindPropertyRelative(nameof(AttackTimingComponent.startTicks));
            EditorGUILayout.PropertyField(startTicks);
            if (kind == WeaponKind.Ranged)
            {
                AnimationTickConverter.DrawRange(EditorGUILayout.GetControlRect(false, AnimationTickConverter.RangeHeight), startTicks, null, null);
                return;
            }

            SerializedProperty durationTicks = timing.FindPropertyRelative(nameof(AttackTimingComponent.durationTicks));
            if (kind == WeaponKind.Throw)
            {
                EditorGUILayout.PropertyField(durationTicks);
                AnimationTickConverter.DrawRange(EditorGUILayout.GetControlRect(false, AnimationTickConverter.RangeHeight), startTicks, null, durationTicks);
                return;
            }

            SerializedProperty endTicks = timing.FindPropertyRelative(nameof(AttackTimingComponent.endTicks));
            EditorGUILayout.PropertyField(endTicks);
            EditorGUILayout.PropertyField(durationTicks);
            AnimationTickConverter.DrawRange(EditorGUILayout.GetControlRect(false, AnimationTickConverter.RangeHeight), startTicks, endTicks, durationTicks);
        }

        // 부모 칸 안의 자식 칸 하나를 자식까지 펼쳐 그린다.
        private static void DrawRelative(SerializedProperty parent, string propertyName)
        {
            EditorGUILayout.PropertyField(parent.FindPropertyRelative(propertyName), true);
        }

        // 부모 칸 안의 자식 칸 하나를 주어진 이름으로 그린다.
        private static void DrawRelative(SerializedProperty parent, string propertyName, string label)
        {
            EditorGUILayout.PropertyField(parent.FindPropertyRelative(propertyName), new GUIContent(label), true);
        }
    }
}
