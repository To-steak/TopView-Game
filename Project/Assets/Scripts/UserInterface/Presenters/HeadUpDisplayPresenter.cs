using TopView.Components;
using TopView.UserInterface.Views;
using UnityEngine;

namespace TopView.UserInterface.Presenters
{
    // 플레이어가 살아 있는 동안 HeadUpDisplayView를 보이고, Runner가 매 프레임 넘기는 체력과 탄약을 보여 준다.
    public class HeadUpDisplayPresenter : MonoBehaviour
    {
        [SerializeField] private HeadUpDisplayView view;

        private bool _isShown;
        // 같은 값을 매 프레임 다시 쓰지 않도록 마지막으로 보여 준 값을 기억한다
        private bool _hasPresentedValues;
        private float _presentedHealth;
        private float _presentedMaxHealth;
        private bool _presentedHasWeapon;
        private WeaponKind _presentedWeaponKind;
        private int _presentedCurrentAmmo;
        private int _presentedReserveAmmo;

        private const string NO_WEAPON_TEXT = "No Weapon is here";
        private const string MELEE_AMMO_TEXT = "-";

        /// <summary>
        /// View를 숨기고 보여 준 값을 처음 상태로 되돌린다. 판을 준비할 때 Runner가 부른다.
        /// </summary>
        public void Initialize()
        {
            view.gameObject.SetActive(false);
            _isShown = false;
            _hasPresentedValues = false;
        }

        /// <summary>
        /// 플레이어가 있으면 View를 보이고 체력과 탄약을 보여 준다. 없으면 View를 숨긴다.
        /// </summary>
        /// <param name="hasPlayer">살아 있는 플레이어가 있는지</param>
        /// <param name="health">현재 체력</param>
        /// <param name="maxHealth">최대 체력</param>
        /// <param name="hasWeapon">들고 있는 무기가 있는지</param>
        /// <param name="weaponKind">들고 있는 무기 종류</param>
        /// <param name="currentAmmo">장전된 탄약 수</param>
        /// <param name="reserveAmmo">예비 탄약 수</param>
        public void Present(bool hasPlayer, float health, float maxHealth, bool hasWeapon, WeaponKind weaponKind, int currentAmmo, int reserveAmmo)
        {
            if (!hasPlayer)
            {
                if (_isShown)
                {
                    view.gameObject.SetActive(false);
                    _isShown = false;
                }
                return;
            }

            if (!_isShown)
            {
                view.gameObject.SetActive(true);
                _isShown = true;
            }

            if (!_hasPresentedValues || health != _presentedHealth || maxHealth != _presentedMaxHealth)
            {
                view.SetHealth(health, maxHealth);
                _presentedHealth = health;
                _presentedMaxHealth = maxHealth;
            }

            if (!_hasPresentedValues || hasWeapon != _presentedHasWeapon || weaponKind != _presentedWeaponKind
                || currentAmmo != _presentedCurrentAmmo || reserveAmmo != _presentedReserveAmmo)
            {
                view.SetAmmoText(GetAmmoText(hasWeapon, weaponKind, currentAmmo, reserveAmmo));
                _presentedHasWeapon = hasWeapon;
                _presentedWeaponKind = weaponKind;
                _presentedCurrentAmmo = currentAmmo;
                _presentedReserveAmmo = reserveAmmo;
            }

            _hasPresentedValues = true;
        }

        private static string GetAmmoText(bool hasWeapon, WeaponKind weaponKind, int currentAmmo, int reserveAmmo)
        {
            if (!hasWeapon)
            {
                return NO_WEAPON_TEXT;
            }

            switch (weaponKind)
            {
                case WeaponKind.Ranged:
                    return $"{currentAmmo} / {reserveAmmo}";
                case WeaponKind.Throw:
                    return $"{reserveAmmo}";
                default:
                    return MELEE_AMMO_TEXT;
            }
        }
    }
}
