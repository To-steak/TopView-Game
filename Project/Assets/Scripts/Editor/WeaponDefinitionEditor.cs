using TopView.Components;
using TopView.Definitions;
using UnityEditor;
using UnityEngine;

namespace TopView.Editor
{
    // 인스펙터와 Definition Editor 창에서 WeaponDefinition을 그린다. 무기 종류(kind)에 따라 그 종류의 무기가 가질 컴포넌트 칸만 보인다.
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

            SerializedProperty kindProperty = serializedObject.FindProperty(nameof(WeaponDefinition.kind));
            EditorGUILayout.PropertyField(kindProperty);
            WeaponKind kind = (WeaponKind)kindProperty.intValue;

            SerializedProperty weapon = serializedObject.FindProperty(nameof(WeaponDefinition.weapon));
            SerializedProperty upgrade = serializedObject.FindProperty(nameof(WeaponDefinition.upgrade));
            DrawRelative(weapon, nameof(WeaponComponent.damage));

            switch (kind)
            {
                case WeaponKind.Melee:
                    DrawMelee(upgrade);
                    break;
                case WeaponKind.Ranged:
                    DrawRanged(upgrade);
                    break;
                case WeaponKind.Throw:
                    DrawThrow(upgrade);
                    break;
            }

            // 조준 평면의 높이에 모든 종류가 쓴다
            DrawRelative(weapon, nameof(WeaponComponent.aimHeight));

            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(WeaponDefinition.modelPrefab)));
            if (kind == WeaponKind.Melee)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(WeaponDefinition.impactEffectPrefab)));
            }
            else
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(WeaponDefinition.projectilePrefab)));
                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(WeaponDefinition.hitEffectPrefab)));
            }

            serializedObject.ApplyModifiedProperties();
        }

        // 근접 공격과 공격 상태의 길이를 클립 구간 하나로 채우는 변환 칸, 공격 간격, 근접 강화 칸을 그린다.
        private void DrawMelee(SerializedProperty upgrade)
        {
            SerializedProperty meleeAttack = serializedObject.FindProperty(nameof(WeaponDefinition.meleeAttack));
            SerializedProperty durationTicks = serializedObject.FindProperty(nameof(WeaponDefinition.attackDuration)).FindPropertyRelative(nameof(AttackDurationComponent.durationTicks));
            EditorGUILayout.PropertyField(meleeAttack, true);
            EditorGUILayout.PropertyField(durationTicks);
            AnimationTickConverter.DrawRange(EditorGUILayout.GetControlRect(false, AnimationTickConverter.RangeHeight),
                meleeAttack.FindPropertyRelative(nameof(MeleeAttackComponent.startTicks)),
                meleeAttack.FindPropertyRelative(nameof(MeleeAttackComponent.endTicks)),
                durationTicks);
            DrawAttackInterval(upgrade);
            DrawRelative(upgrade, nameof(WeaponUpgradeRuleComponent.meleeRangeRatioPerLevel));
        }

        // 공격 간격, 탄창과 예비 탄약, 직선 투사체 공격, 폭발 강화 칸을 그린다. 발마다의 시작은 Shots의 칸에서 채운다.
        private void DrawRanged(SerializedProperty upgrade)
        {
            DrawAttackInterval(upgrade);
            SerializedProperty magazine = serializedObject.FindProperty(nameof(WeaponDefinition.magazine));
            DrawRelative(magazine, nameof(MagazineComponent.max), MAX_AMMO_LABEL);
            DrawRelative(upgrade, nameof(WeaponUpgradeRuleComponent.maxAmmoRatioPerLevel));
            DrawRelative(magazine, nameof(MagazineComponent.startingCurrent), STARTING_CURRENT_AMMO_LABEL);
            DrawReserveAmmo();
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(WeaponDefinition.straightAttack)), true);
            DrawRelative(upgrade, nameof(WeaponUpgradeRuleComponent.explosionRadiusIncreasePerLevel));
        }

        // 클립 길이로 채우는 공격 상태의 길이, 예비 탄약, 포물선 투사체 공격, 투척 조준 값, 폭발 강화 칸을 그린다. 발마다의 시작은 Shots의 칸에서 채운다.
        private void DrawThrow(SerializedProperty upgrade)
        {
            SerializedProperty durationTicks = serializedObject.FindProperty(nameof(WeaponDefinition.attackDuration)).FindPropertyRelative(nameof(AttackDurationComponent.durationTicks));
            AnimationTickConverter.DrawLengthTicks(EditorGUILayout.GetControlRect(), durationTicks);
            DrawReserveAmmo();
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(WeaponDefinition.arcAttack)), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(WeaponDefinition.throwing)), true);
            DrawRelative(upgrade, nameof(WeaponUpgradeRuleComponent.explosionRadiusIncreasePerLevel));
        }

        private void DrawAttackInterval(SerializedProperty upgrade)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(WeaponDefinition.attackInterval)).FindPropertyRelative(nameof(AttackIntervalComponent.intervalTicks)));
            DrawRelative(upgrade, nameof(WeaponUpgradeRuleComponent.attackIntervalRatioPerLevel));
        }

        private void DrawReserveAmmo()
        {
            DrawRelative(serializedObject.FindProperty(nameof(WeaponDefinition.reserveAmmo)), nameof(ReserveAmmoComponent.startingReserve), STARTING_RESERVE_AMMO_LABEL);
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
