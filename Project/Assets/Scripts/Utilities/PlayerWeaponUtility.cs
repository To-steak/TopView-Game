using TopView.Components;
using TopView.Entities;
using UnityEngine;

namespace TopView.Utilities
{
    // 플레이어 무기의 조건 판정, 재장전, 교체 도구. 언제 부를지는 상태 쪽 System이 정한다.
    public static class PlayerWeaponUtility
    {
        // 투척 충전량의 최댓값. 충전량은 최대 거리에 대한 비율(%)이다
        private const int MAX_THROW_CHARGE = 100;

        /// <summary>
        /// 공격 상태를 끝낼 때인지 판정한다. Ranged는 투사체를 내보내면 바로 끝나고, Melee와 Throw는 공격이 나간 뒤 timing.durationTicks까지 기다린다.
        /// </summary>
        /// <param name="actionState">행동 상태와 공격 진행</param>
        /// <param name="weapon">들고 있는 무기</param>
        /// <returns>공격 상태를 끝낼 때면 true</returns>
        public static bool IsAttackFinished(in PlayerActionStateComponent actionState, in WeaponComponent weapon)
        {
            if (!actionState.isAttackCommitted)
            {
                return false;
            }

            return weapon.kind == WeaponKind.Ranged || actionState.elapsedTicks >= weapon.timing.durationTicks;
        }

        /// <summary>
        /// 들고 있는 무기로 들어갈 공격 상태를 구한다.
        /// </summary>
        /// <param name="weapons">무기 목록</param>
        /// <param name="weaponSlots">든 무기와 무기별 탄약</param>
        /// <param name="attackState">들어갈 공격 상태</param>
        /// <returns>들어갈 수 있으면 true. 탄약이 없거나 무기가 없으면 false</returns>
        public static bool TryGetAttackState(WeaponComponent[] weapons, in PlayerWeaponSlotsComponent weaponSlots, out PlayerState attackState)
        {
            attackState = PlayerState.Idle;
            if (!IsValidWeaponIndex(weapons, weaponSlots.currentIndex))
            {
                return false;
            }

            ref readonly WeaponComponent weapon = ref weapons[weaponSlots.currentIndex];
            switch (weapon.kind)
            {
                case WeaponKind.Melee:
                    attackState = PlayerState.Swing;
                    return true;

                case WeaponKind.Ranged:
                    attackState = PlayerState.Shot;
                    return weaponSlots.currentAmmo[weaponSlots.currentIndex] > 0;

                case WeaponKind.Throw:
                    attackState = PlayerState.ThrowCharge;
                    return weaponSlots.reserveAmmo[weaponSlots.currentIndex] > 0;
            }

            return false;
        }

        /// <summary>
        /// 충전한 시간으로 투척 충전량을 구한다. 1틱에 chargePerTick씩 오르고, 100에 닿으면 같은 빠르기로 내려가 0까지 돌아오기를 되풀이한다. 소수점 아래는 버린다.
        /// </summary>
        /// <param name="chargeTicks">충전한 시간(틱)</param>
        /// <param name="chargePerTick">1틱에 오르는 충전량(%)</param>
        /// <returns>충전량(0~100). chargePerTick이 0 이하면 0</returns>
        public static int GetThrowCharge(int chargeTicks, float chargePerTick)
        {
            if (chargePerTick <= 0f)
            {
                return 0;
            }

            float cycleCharge = chargeTicks * chargePerTick % (MAX_THROW_CHARGE * 2);
            float charge = cycleCharge <= MAX_THROW_CHARGE ? cycleCharge : MAX_THROW_CHARGE * 2 - cycleCharge;
            return Mathf.Clamp(Mathf.FloorToInt(charge), 0, MAX_THROW_CHARGE);
        }

        /// <summary>
        /// 충전량에 비례한 거리로 착탄점을 구한다. 던지는 쪽의 앞 방향(XZ)으로 거리만큼 떨어진 곳이며, 높이는 던지는 쪽의 발 높이다.
        /// </summary>
        /// <param name="projectile">던질 투사체</param>
        /// <param name="position">던지는 쪽의 위치(발)</param>
        /// <param name="rotation">던지는 쪽의 회전</param>
        /// <param name="throwCharge">투척 충전량(0~100)</param>
        /// <returns>착탄점(월드)</returns>
        public static Vector3 GetThrowTargetPosition(in ProjectileComponent projectile, Vector3 position, Quaternion rotation, int throwCharge)
        {
            Vector3 forward = rotation * Vector3.forward;
            forward.y = 0f;
            forward.Normalize();

            float distance = Mathf.Max(0f, projectile.maxThrowDistance) * Mathf.Clamp(throwCharge, 0, MAX_THROW_CHARGE) / MAX_THROW_CHARGE;
            return position + forward * distance;
        }

        /// <summary>
        /// 그 번호의 무기로 교체할 수 있는지 판정한다.
        /// </summary>
        /// <param name="weapons">무기 목록</param>
        /// <param name="weaponSlots">든 무기와 무기별 탄약</param>
        /// <param name="weaponIndex">바꿀 무기 번호</param>
        /// <returns>들고 있지 않은 유효한 무기면 true</returns>
        public static bool CanSwap(WeaponComponent[] weapons, in PlayerWeaponSlotsComponent weaponSlots, int weaponIndex)
        {
            return weaponIndex != weaponSlots.currentIndex && IsValidWeaponIndex(weapons, weaponIndex);
        }

        /// <summary>
        /// 교체 상태에서 swapStartTicks가 되면 무기를 바꾸고 연사 대기를 지운다. swapStartTicks가 상태 길이보다 길어도 상태가 끝나기 전에 바꾼다.
        /// </summary>
        /// <param name="actionState">행동 상태와 공격 진행</param>
        /// <param name="weaponSlots">든 무기와 무기별 탄약</param>
        /// <param name="action">행동 상태의 길이</param>
        public static void CommitSwap(ref PlayerActionStateComponent actionState, ref PlayerWeaponSlotsComponent weaponSlots, in PlayerActionComponent action)
        {
            if (weaponSlots.currentIndex != weaponSlots.pendingIndex && actionState.elapsedTicks >= Mathf.Min(action.swapStartTicks, action.swapTicks))
            {
                weaponSlots.currentIndex = weaponSlots.pendingIndex;
                actionState.attackCooldownTicks = 0;
            }
        }

        /// <summary>
        /// 들고 있는 무기를 재장전할 수 있는지 판정한다.
        /// </summary>
        /// <param name="weapons">무기 목록</param>
        /// <param name="weaponSlots">든 무기와 무기별 탄약</param>
        /// <param name="upgradeLevels">강화 레벨. 최대 탄약 수에 반영한다</param>
        /// <returns>Ranged이고 탄창이 덜 찼으며 예비 탄약이 있으면 true</returns>
        public static bool CanReload(WeaponComponent[] weapons, in PlayerWeaponSlotsComponent weaponSlots, in PlayerUpgradeLevelsComponent upgradeLevels)
        {
            if (!IsValidWeaponIndex(weapons, weaponSlots.currentIndex))
            {
                return false;
            }

            int weaponIndex = weaponSlots.currentIndex;
            ref readonly WeaponComponent weapon = ref weapons[weaponIndex];
            int maxAmmo = PlayerUpgradeUtility.GetMaxAmmo(in weapon, PlayerUpgradeUtility.GetWeaponLevel(in upgradeLevels, weaponIndex));
            return weapon.kind == WeaponKind.Ranged && weaponSlots.currentAmmo[weaponIndex] < maxAmmo && weaponSlots.reserveAmmo[weaponIndex] > 0;
        }

        /// <summary>
        /// 예비 탄약에서 모자란 만큼 장전한다.
        /// </summary>
        /// <param name="weapons">무기 목록</param>
        /// <param name="weaponSlots">든 무기와 무기별 탄약</param>
        /// <param name="upgradeLevels">강화 레벨. 최대 탄약 수에 반영한다</param>
        public static void Reload(WeaponComponent[] weapons, ref PlayerWeaponSlotsComponent weaponSlots, in PlayerUpgradeLevelsComponent upgradeLevels)
        {
            int weaponIndex = weaponSlots.currentIndex;
            int maxAmmo = PlayerUpgradeUtility.GetMaxAmmo(in weapons[weaponIndex], PlayerUpgradeUtility.GetWeaponLevel(in upgradeLevels, weaponIndex));
            int loadAmmo = Mathf.Min(maxAmmo - weaponSlots.currentAmmo[weaponIndex], weaponSlots.reserveAmmo[weaponIndex]);
            weaponSlots.currentAmmo[weaponIndex] += loadAmmo;
            weaponSlots.reserveAmmo[weaponIndex] -= loadAmmo;
        }

        private static bool IsValidWeaponIndex(WeaponComponent[] weapons, int weaponIndex)
        {
            return weaponIndex >= 0 && weaponIndex < weapons.Length;
        }
    }
}
