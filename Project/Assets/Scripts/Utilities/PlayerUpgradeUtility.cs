using TopView.Components;
using UnityEngine;

namespace TopView.Utilities
{
    // 플레이어 강화 레벨로 비용과 강화된 수치를 계산하는 도구.
    public static class PlayerUpgradeUtility
    {
        /// <summary>
        /// 지금 레벨에서 다음 레벨로 강화하는 비용을 돌려준다.
        /// </summary>
        /// <param name="upgrade">강화 값</param>
        /// <param name="level">지금 레벨</param>
        /// <returns>startingCost + costIncreasePerLevel × level</returns>
        public static int GetCost(in PlayerUpgradeComponent upgrade, int level)
        {
            return upgrade.startingCost + upgrade.costIncreasePerLevel * level;
        }

        /// <summary>
        /// 레벨만큼 비율로 오른 값을 돌려준다.
        /// </summary>
        /// <param name="baseValue">Definition의 값</param>
        /// <param name="ratioPerLevel">레벨마다 오르는 비율</param>
        /// <param name="level">강화 레벨</param>
        /// <returns>baseValue × (1 + ratioPerLevel × level)</returns>
        public static float Increase(float baseValue, float ratioPerLevel, int level)
        {
            return baseValue * (1f + ratioPerLevel * level);
        }

        /// <summary>
        /// 레벨만큼 비율로 줄어든 값을 돌려준다. 0 아래로는 줄지 않는다.
        /// </summary>
        /// <param name="baseValue">Definition의 값</param>
        /// <param name="ratioPerLevel">레벨마다 줄어드는 비율</param>
        /// <param name="level">강화 레벨</param>
        /// <returns>baseValue × max(0, 1 - ratioPerLevel × level)</returns>
        public static float Decrease(float baseValue, float ratioPerLevel, int level)
        {
            return baseValue * Mathf.Max(0f, 1f - ratioPerLevel * level);
        }

        /// <summary>
        /// 강화가 반영된 달리기 마나 소모량(틱당)을 돌려준다.
        /// </summary>
        /// <param name="vitals">체력과 마나 값</param>
        /// <param name="upgrade">강화 값</param>
        /// <param name="levels">강화 레벨</param>
        /// <returns>달리기 마나 소모량(틱당)</returns>
        public static float GetRunManaCost(in PlayerVitalsComponent vitals, in PlayerUpgradeComponent upgrade, in PlayerUpgradeLevelsComponent levels)
        {
            return Decrease(vitals.runManaCostPerTick, upgrade.runManaCostRatioPerLevel, levels.runManaCostLevel);
        }

        /// <summary>
        /// 강화가 반영된 회피 마나 소모량을 돌려준다.
        /// </summary>
        /// <param name="vitals">체력과 마나 값</param>
        /// <param name="upgrade">강화 값</param>
        /// <param name="levels">강화 레벨</param>
        /// <returns>회피 한 번의 마나 소모량</returns>
        public static float GetDodgeManaCost(in PlayerVitalsComponent vitals, in PlayerUpgradeComponent upgrade, in PlayerUpgradeLevelsComponent levels)
        {
            return Decrease(vitals.dodgeManaCost, upgrade.dodgeManaCostRatioPerLevel, levels.dodgeManaCostLevel);
        }

        /// <summary>
        /// 강화가 반영된 점프 마나 소모량을 돌려준다.
        /// </summary>
        /// <param name="vitals">체력과 마나 값</param>
        /// <param name="upgrade">강화 값</param>
        /// <param name="levels">강화 레벨</param>
        /// <returns>점프 한 번의 마나 소모량</returns>
        public static float GetJumpManaCost(in PlayerVitalsComponent vitals, in PlayerUpgradeComponent upgrade, in PlayerUpgradeLevelsComponent levels)
        {
            return Decrease(vitals.jumpManaCost, upgrade.jumpManaCostRatioPerLevel, levels.jumpManaCostLevel);
        }

        /// <summary>
        /// 무기 하나의 강화 레벨을 돌려준다. 무기 번호는 무기 목록과 같은 번호여야 한다.
        /// </summary>
        /// <param name="levels">강화 레벨</param>
        /// <param name="weaponIndex">무기 번호</param>
        /// <returns>무기 강화 레벨</returns>
        public static int GetWeaponLevel(in PlayerUpgradeLevelsComponent levels, int weaponIndex)
        {
            // 레벨 배열은 생성 때 무기 목록과 길이를 맞춘다. 번호가 배열 밖이면 0으로 대신하지 않고 예외로 드러낸다
            return levels.weaponLevels[weaponIndex];
        }

        /// <summary>
        /// 강화가 반영된 무기 피해량을 돌려준다.
        /// </summary>
        /// <param name="weapon">무기</param>
        /// <param name="upgrade">강화 값</param>
        /// <param name="weaponLevel">무기 강화 레벨</param>
        /// <returns>한 번 맞을 때의 피해량</returns>
        public static float GetDamage(in WeaponComponent weapon, in PlayerUpgradeComponent upgrade, int weaponLevel)
        {
            return Increase(weapon.damage, upgrade.damageRatioPerLevel, weaponLevel);
        }

        /// <summary>
        /// 강화가 반영된 공격 간격(틱)을 돌려준다. 반올림하며, 최소 공격 간격 아래로는 줄지 않는다. 처음부터 최소보다 짧으면 처음 값을 돌려준다.
        /// </summary>
        /// <param name="weapon">무기</param>
        /// <param name="upgrade">강화 값</param>
        /// <param name="weaponLevel">무기 강화 레벨</param>
        /// <returns>공격 간격(틱)</returns>
        public static int GetAttackIntervalTicks(in WeaponComponent weapon, in PlayerUpgradeComponent upgrade, int weaponLevel)
        {
            int intervalTicks = Mathf.RoundToInt(Decrease(weapon.attackIntervalTicks, weapon.attackIntervalRatioPerLevel, weaponLevel));
            return Mathf.Max(intervalTicks, Mathf.Min(weapon.attackIntervalTicks, upgrade.minAttackIntervalTicks));
        }

        /// <summary>
        /// 강화가 반영된 최대 탄약 수를 돌려준다. 반올림한다.
        /// </summary>
        /// <param name="weapon">무기</param>
        /// <param name="weaponLevel">무기 강화 레벨</param>
        /// <returns>장전할 수 있는 최대 탄약 수</returns>
        public static int GetMaxAmmo(in WeaponComponent weapon, int weaponLevel)
        {
            return Mathf.RoundToInt(Increase(weapon.maxAmmo, weapon.maxAmmoRatioPerLevel, weaponLevel));
        }

        /// <summary>
        /// 강화가 반영된 투사체 폭발 반지름을 돌려준다.
        /// </summary>
        /// <param name="weapon">무기</param>
        /// <param name="weaponLevel">무기 강화 레벨</param>
        /// <returns>폭발 반지름(유닛, XZ). 0이면 폭발하지 않는다</returns>
        public static float GetExplosionRadius(in WeaponComponent weapon, int weaponLevel)
        {
            return weapon.projectile.explosionRadius + weapon.explosionRadiusIncreasePerLevel * weaponLevel;
        }

        /// <summary>
        /// 근접 판정 반지름, 앞쪽으로 띄우는 거리, 임팩트 이펙트 크기에 곱할 배율을 돌려준다.
        /// </summary>
        /// <param name="weapon">무기</param>
        /// <param name="weaponLevel">무기 강화 레벨</param>
        /// <returns>1 + meleeRangeRatioPerLevel × weaponLevel</returns>
        public static float GetMeleeRangeScale(in WeaponComponent weapon, int weaponLevel)
        {
            return Increase(1f, weapon.meleeRangeRatioPerLevel, weaponLevel);
        }
    }
}
