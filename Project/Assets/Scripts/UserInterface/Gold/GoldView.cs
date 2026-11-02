using TMPro;
using UnityEngine;

namespace TopView.UserInterface.Gold
{
    // 번 골드를 보여 준다.
    public class GoldView : UserInterfaceView
    {
        [SerializeField] private TMP_Text _goldText;

        /// <summary>
        /// 골드를 천 단위 구분 기호와 함께 보여 준다.
        /// </summary>
        /// <param name="gold">골드</param>
        public void SetGold(int gold)
        {
            _goldText.text = gold.ToString("N0");
        }
    }
}
