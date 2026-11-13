using TopView.Components;
using TopView.Entities;
using UnityEngine;

namespace TopView.Utilities
{
    // 플레이어 무기의 조건 판정, 재장전, 교체 도구. 언제 부를지는 상태 쪽 System이 정한다. 무기가 할 수 있는 일은 무기가 가진 컴포넌트로 정한다.
    public static class PlayerWeaponUtility
    {
        // 투척 충전량의 최댓값. 충전량은 최대 거리에 대한 비율(%)이다
        private const int MAX_THROW_CHARGE = 100;
        // 무기를 들고 공격할 수 있는 주인이 가진 컴포넌트
        private const ComponentMask ATTACKER_MASK = ComponentMask.PlayerActionState | ComponentMask.WeaponSlots;
        // 재장전할 수 있는 무기가 가진 컴포넌트
        private const ComponentMask RELOAD_MASK = ComponentMask.Magazine | ComponentMask.MagazineState | ComponentMask.ReserveAmmoState | ComponentMask.WeaponUpgrade | ComponentMask.UpgradeLevel;

        /// <summary>
        /// 공격 상태를 끝낼 때인지 판정한다. 공격이 모두 나간 뒤, AttackDuration이 있는 무기는 durationTicks까지 기다리고 없는 무기는 바로 끝난다.
        /// </summary>
        /// <param name="actionState">행동 상태와 공격 진행</param>
        /// <param name="weapon">들고 있는 무기 엔티티</param>
        /// <returns>공격 상태를 끝낼 때면 true</returns>
        public static bool IsAttackFinished(in PlayerActionStateComponent actionState, in Entity weapon)
        {
            if (actionState.committedShotCount < GetShotCount(in weapon))
            {
                return false;
            }

            return (weapon.mask & ComponentMask.AttackDuration) == 0 || actionState.elapsedTicks >= weapon.attackDuration.durationTicks;
        }

        /// <summary>
        /// 무기의 공격 한 번에 내보내는 발 수를 돌려준다. 무기가 가진 판정 방식 컴포넌트의 발 수다.
        /// </summary>
        /// <param name="weapon">무기 엔티티</param>
        /// <returns>내보내는 발 수. 판정 방식 컴포넌트가 없으면 0</returns>
        public static int GetShotCount(in Entity weapon)
        {
            if ((weapon.mask & ComponentMask.MeleeAttack) != 0)
            {
                return AttackUtility.GetShotCount(in weapon.meleeAttack);
            }
            if ((weapon.mask & ComponentMask.StraightAttack) != 0)
            {
                return AttackUtility.GetShotCount(in weapon.straightAttack);
            }
            if ((weapon.mask & ComponentMask.ArcAttack) != 0)
            {
                return AttackUtility.GetShotCount(in weapon.arcAttack);
            }

            return 0;
        }

        /// <summary>
        /// 들고 있는 무기로 들어갈 공격 상태를 구한다. 탄창이 있는 무기는 장전된 탄약이, 탄창 없이 예비 탄약만 쓰는 무기는 예비 탄약이 있어야 한다.
        /// </summary>
        /// <param name="weapon">들고 있는 무기 엔티티</param>
        /// <param name="attackState">들어갈 공격 상태</param>
        /// <returns>들어갈 수 있으면 true. 탄약이 없거나 AttackState가 없으면 false</returns>
        public static bool TryGetAttackState(in Entity weapon, out PlayerState attackState)
        {
            attackState = PlayerState.Idle;
            if ((weapon.mask & ComponentMask.AttackState) == 0)
            {
                return false;
            }

            attackState = weapon.attackState.state;
            if ((weapon.mask & ComponentMask.MagazineState) != 0)
            {
                return weapon.magazineState.current > 0;
            }
            if ((weapon.mask & ComponentMask.ReserveAmmoState) != 0)
            {
                return weapon.reserveAmmoState.reserve > 0;
            }

            return true;
        }

        /// <summary>
        /// 주인이 지금 이 무기를 들고 공격 상태(Shot, Swing, Throw)에 있는지 판정한다.
        /// </summary>
        /// <param name="owner">무기 주인의 칸</param>
        /// <param name="ownerOfWeapon">무기가 가리키는 주인</param>
        /// <param name="weapon">무기 자신의 식별자</param>
        /// <returns>주인이 살아 있는 플레이어이고, 든 무기가 이 무기이며, 공격 상태면 true</returns>
        public static bool IsAttackingWith(in Entity owner, in OwnerComponent ownerOfWeapon, in EntityIdentifier weapon)
        {
            if ((owner.mask & ATTACKER_MASK) != ATTACKER_MASK || owner.generation != ownerOfWeapon.owner.generation)
            {
                return false;
            }

            PlayerState state = owner.playerActionState.state;
            if (state != PlayerState.Shot && state != PlayerState.Swing && state != PlayerState.Throw)
            {
                return false;
            }

            EntityIdentifier[] weapons = owner.weaponSlots.weapons;
            int currentIndex = owner.weaponSlots.currentIndex;
            return currentIndex >= 0 && currentIndex < weapons.Length
                && weapons[currentIndex].index == weapon.index && weapons[currentIndex].generation == weapon.generation;
        }

        /// <summary>
        /// 공격 한 번에 쓸 탄약 하나를 쓴다. 탄창이 있는 무기는 장전된 탄약을, 탄창 없이 예비 탄약만 있는 무기는 예비 탄약을 쓰고, 탄약이 없는 무기는 쓰지 않는다.
        /// </summary>
        /// <param name="weapon">무기 엔티티</param>
        /// <param name="remainingAmmo">쓰기 전의 탄약 수. 탄약이 없는 무기면 -1</param>
        /// <returns>쓸 탄약이 있었거나 탄약이 없는 무기면 true. 탄약이 떨어졌으면 false</returns>
        public static bool TryUseAmmo(ref Entity weapon, out int remainingAmmo)
        {
            remainingAmmo = -1;
            if ((weapon.mask & ComponentMask.MagazineState) != 0)
            {
                remainingAmmo = weapon.magazineState.current;
                if (remainingAmmo <= 0)
                {
                    return false;
                }
                weapon.magazineState.current--;
                return true;
            }
            if ((weapon.mask & ComponentMask.ReserveAmmoState) != 0)
            {
                remainingAmmo = weapon.reserveAmmoState.reserve;
                if (remainingAmmo <= 0)
                {
                    return false;
                }
                weapon.reserveAmmoState.reserve--;
                return true;
            }

            return true;
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
        /// <param name="throwing">던지는 무기의 조준 값</param>
        /// <param name="position">던지는 쪽의 위치(발)</param>
        /// <param name="rotation">던지는 쪽의 회전</param>
        /// <param name="throwCharge">투척 충전량(0~100)</param>
        /// <returns>착탄점(월드)</returns>
        public static Vector3 GetThrowTargetPosition(in ThrowComponent throwing, Vector3 position, Quaternion rotation, int throwCharge)
        {
            Vector3 forward = rotation * Vector3.forward;
            forward.y = 0f;
            forward.Normalize();

            float distance = Mathf.Max(0f, throwing.maxThrowDistance) * Mathf.Clamp(throwCharge, 0, MAX_THROW_CHARGE) / MAX_THROW_CHARGE;
            return position + forward * distance;
        }

        /// <summary>
        /// 그 번호의 무기로 교체할 수 있는지 판정한다.
        /// </summary>
        /// <param name="weaponSlots">든 무기</param>
        /// <param name="weaponIndex">바꿀 무기 번호</param>
        /// <returns>들고 있지 않은 유효한 무기면 true</returns>
        public static bool CanSwap(in PlayerWeaponSlotsComponent weaponSlots, int weaponIndex)
        {
            return weaponIndex != weaponSlots.currentIndex && weaponIndex >= 0 && weaponIndex < weaponSlots.weapons.Length;
        }

        /// <summary>
        /// 교체 상태에서 swapStartTicks가 되면 무기를 바꾸고 연사 대기를 지운다. swapStartTicks가 상태 길이보다 길어도 상태가 끝나기 전에 바꾼다.
        /// </summary>
        /// <param name="actionState">행동 상태와 공격 진행</param>
        /// <param name="weaponSlots">든 무기</param>
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
        /// <param name="weapon">들고 있는 무기 엔티티</param>
        /// <returns>탄창과 예비 탄약이 있는 무기이고, 탄창이 덜 찼으며 예비 탄약이 남았으면 true</returns>
        public static bool CanReload(in Entity weapon)
        {
            if ((weapon.mask & RELOAD_MASK) != RELOAD_MASK)
            {
                return false;
            }

            int maxAmmo = PlayerUpgradeUtility.GetMaxAmmo(in weapon.magazine, in weapon.weaponUpgrade, weapon.upgradeLevel.level);
            return weapon.magazineState.current < maxAmmo && weapon.reserveAmmoState.reserve > 0;
        }

        /// <summary>
        /// 예비 탄약에서 모자란 만큼 탄창에 장전한다.
        /// </summary>
        /// <param name="magazineState">장전된 탄약 수</param>
        /// <param name="reserveAmmoState">예비 탄약 수</param>
        /// <param name="maxAmmo">강화가 반영된 최대 탄약 수</param>
        public static void Reload(ref MagazineStateComponent magazineState, ref ReserveAmmoStateComponent reserveAmmoState, int maxAmmo)
        {
            int loadAmmo = Mathf.Min(maxAmmo - magazineState.current, reserveAmmoState.reserve);
            magazineState.current += loadAmmo;
            reserveAmmoState.reserve -= loadAmmo;
        }
    }
}
