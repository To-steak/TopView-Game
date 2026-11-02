using UnityEngine;

namespace TopView.UserInterface.PlayerMana
{
    // 플레이어가 살아 있는 동안 PlayerManaView를 보이고, Runner가 매 프레임 넘기는 마나를 보여 준다.
    public class PlayerManaPresenter : MonoBehaviour
    {
        [SerializeField] private PlayerManaView _view;

        private bool _isShown;
        // 같은 값을 매 프레임 다시 쓰지 않도록 마지막으로 보여 준 값을 기억한다
        private bool _hasPresentedValues;
        private float _presentedMana;
        private float _presentedMaxMana;

        /// <summary>
        /// View를 숨기고 보여 준 값을 처음 상태로 되돌린다. 판을 준비할 때 Runner가 부른다.
        /// </summary>
        public void Initialize()
        {
            _view.SetVisible(false);
            _isShown = false;
            _hasPresentedValues = false;
        }

        /// <summary>
        /// 플레이어가 있으면 View를 보이고 마나를 보여 준다. 없으면 View를 숨긴다.
        /// </summary>
        /// <param name="hasPlayer">살아 있는 플레이어가 있는지</param>
        /// <param name="mana">현재 마나</param>
        /// <param name="maxMana">최대 마나</param>
        public void Present(bool hasPlayer, float mana, float maxMana)
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

            if (!_hasPresentedValues || mana != _presentedMana || maxMana != _presentedMaxMana)
            {
                _view.SetMana(mana, maxMana);
                _presentedMana = mana;
                _presentedMaxMana = maxMana;
                _hasPresentedValues = true;
            }
        }
    }
}
