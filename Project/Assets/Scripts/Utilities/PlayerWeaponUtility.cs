using TopView.Components;
using TopView.Entities;
using UnityEngine;

namespace TopView.Systems
{
    // 플레이어 무기의 조건 판정, 재장전, 교체 도구. 언제 부를지는 상태 쪽 System이 정한다.
    public static class PlayerWeaponUtility
    {
        /// <summary>
        /// 공격 상태를 끝낼 때인지 판정한다. Ranged는 투사체를 내보내면 바로 끝나고, Melee와 Throw는 공격이 나간 뒤 attackDurationTicks까지 기다린다.
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

            return weapon.kind == WeaponKind.Ranged || actionState.elapsedTicks >= weapon.attackDurationTicks;
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
                    attackState = PlayerState.Throw;
                    return weaponSlots.reserveAmmo[weaponSlots.currentIndex] > 0;
            }

            return false;
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
        /// <returns>Ranged이고 탄창이 덜 찼으며 예비 탄약이 있으면 true</returns>
        public static bool CanReload(WeaponComponent[] weapons, in PlayerWeaponSlotsComponent weaponSlots)
        {
            if (!IsValidWeaponIndex(weapons, weaponSlots.currentIndex))
            {
                return false;
            }

            int weaponIndex = weaponSlots.currentIndex;
            ref readonly WeaponComponent weapon = ref weapons[weaponIndex];
            return weapon.kind == WeaponKind.Ranged && weaponSlots.currentAmmo[weaponIndex] < weapon.maxAmmo && weaponSlots.reserveAmmo[weaponIndex] > 0;
        }

        /// <summary>
        /// 예비 탄약에서 모자란 만큼 장전한다.
        /// </summary>
        /// <param name="weapons">무기 목록</param>
        /// <param name="weaponSlots">든 무기와 무기별 탄약</param>
        public static void Reload(WeaponComponent[] weapons, ref PlayerWeaponSlotsComponent weaponSlots)
        {
            int weaponIndex = weaponSlots.currentIndex;
            int loadAmmo = Mathf.Min(weapons[weaponIndex].maxAmmo - weaponSlots.currentAmmo[weaponIndex], weaponSlots.reserveAmmo[weaponIndex]);
            weaponSlots.currentAmmo[weaponIndex] += loadAmmo;
            weaponSlots.reserveAmmo[weaponIndex] -= loadAmmo;
        }

        private static bool IsValidWeaponIndex(WeaponComponent[] weapons, int weaponIndex)
        {
            return weaponIndex >= 0 && weaponIndex < weapons.Length;
        }
    }
}
