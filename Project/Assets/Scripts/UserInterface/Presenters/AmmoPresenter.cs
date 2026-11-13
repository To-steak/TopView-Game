using TopView.UserInterface.Views;
using UnityEngine;

namespace TopView.UserInterface.Presenters
{
    // 플레이어가 살아 있는 동안 AmmoView를 보이고, World가 매 프레임 넘기는 무기와 탄약을 글자로 보여 준다.
    public class AmmoPresenter : MonoBehaviour
    {
        [SerializeField] private AmmoView _view;

        private bool _isShown;
        // 같은 값을 매 프레임 다시 쓰지 않도록 마지막으로 보여 준 값을 기억한다
        private bool _hasPresentedValues;
        private bool _hasPresentedWeapon;
        private bool _hasPresentedMagazine;
        private bool _hasPresentedReserveAmmo;
        private int _presentedCurrentAmmo;
        private int _presentedReserveAmmo;

        private const string NO_WEAPON_TEXT = "No Weapon is here";
        private const string MELEE_AMMO_TEXT = "-";

        /// <summary>
        /// View를 숨기고 보여 준 값을 처음 상태로 되돌린다. 판을 준비할 때 World가 부른다.
        /// </summary>
        public void Initialize()
        {
            _view.SetVisible(false);
            _isShown = false;
            _hasPresentedValues = false;
        }

        /// <summary>
        /// 플레이어가 있으면 View를 보이고 탄약을 보여 준다. 없으면 View를 숨긴다.
        /// </summary>
        /// <param name="hasPlayer">살아 있는 플레이어가 있는지</param>
        /// <param name="hasWeapon">들고 있는 무기가 있는지</param>
        /// <param name="hasMagazine">들고 있는 무기에 탄창이 있는지</param>
        /// <param name="hasReserveAmmo">들고 있는 무기에 예비 탄약이 있는지</param>
        /// <param name="currentAmmo">장전된 탄약 수</param>
        /// <param name="reserveAmmo">예비 탄약 수</param>
        public void Present(bool hasPlayer, bool hasWeapon, bool hasMagazine, bool hasReserveAmmo, int currentAmmo, int reserveAmmo)
        {
            if (!hasPlayer)
            {
                if (_isShown)
                {
                    _view.SetVisible(false);
                    _isShown = false;
                }
                return;
            }

            if (!_isShown)
            {
                _view.SetVisible(true);
                _isShown = true;
            }

            if (!_hasPresentedValues || hasWeapon != _hasPresentedWeapon || hasMagazine != _hasPresentedMagazine || hasReserveAmmo != _hasPresentedReserveAmmo
                || currentAmmo != _presentedCurrentAmmo || reserveAmmo != _presentedReserveAmmo)
            {
                _view.SetAmmoText(GetAmmoText(hasWeapon, hasMagazine, hasReserveAmmo, currentAmmo, reserveAmmo));
                _hasPresentedWeapon = hasWeapon;
                _hasPresentedMagazine = hasMagazine;
                _hasPresentedReserveAmmo = hasReserveAmmo;
                _presentedCurrentAmmo = currentAmmo;
                _presentedReserveAmmo = reserveAmmo;
                _hasPresentedValues = true;
            }
        }

        // 탄창이 있으면 "장전 / 예비", 예비 탄약만 있으면 예비 탄약 수, 탄약이 없는 무기면 "-"를 보여 준다
        private static string GetAmmoText(bool hasWeapon, bool hasMagazine, bool hasReserveAmmo, int currentAmmo, int reserveAmmo)
        {
            if (!hasWeapon)
            {
                return NO_WEAPON_TEXT;
            }
            if (hasMagazine)
            {
                return $"{currentAmmo} / {reserveAmmo}";
            }
            if (hasReserveAmmo)
            {
                return $"{reserveAmmo}";
            }

            return MELEE_AMMO_TEXT;
        }
    }
}
