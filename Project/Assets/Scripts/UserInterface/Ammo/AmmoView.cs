using TMPro;
using UnityEngine;

namespace TopView.UserInterface.Ammo
{
    // 들고 있는 무기의 탄약을 글자로 보여 준다.
    public class AmmoView : UserInterfaceView
    {
        [SerializeField] private TMP_Text _ammoText;

        /// <summary>
        /// 탄약 글자를 보여 준다.
        /// </summary>
        /// <param name="text">보여 줄 글자</param>
        public void SetAmmoText(string text)
        {
            _ammoText.text = text;
        }
    }
}
