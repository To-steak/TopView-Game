using UnityEngine;

namespace TopView.UserInterface.PlayerHealth
{
    // 플레이어가 살아 있는 동안 PlayerHealthView를 보이고, Runner가 매 프레임 넘기는 체력을 보여 준다.
    public class PlayerHealthPresenter : MonoBehaviour
    {
        [SerializeField] private PlayerHealthView _view;

        private bool _isShown;
        // 같은 값을 매 프레임 다시 쓰지 않도록 마지막으로 보여 준 값을 기억한다
        private bool _hasPresentedValues;
        private float _presentedHealth;
        private float _presentedMaxHealth;

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
        /// 플레이어가 있으면 View를 보이고 체력을 보여 준다. 없으면 View를 숨긴다.
        /// </summary>
        /// <param name="hasPlayer">살아 있는 플레이어가 있는지</param>
        /// <param name="health">현재 체력</param>
        /// <param name="maxHealth">최대 체력</param>
        public void Present(bool hasPlayer, float health, float maxHealth)
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

            if (!_hasPresentedValues || health != _presentedHealth || maxHealth != _presentedMaxHealth)
            {
                _view.SetHealth(health, maxHealth);
                _presentedHealth = health;
                _presentedMaxHealth = maxHealth;
                _hasPresentedValues = true;
            }
        }
    }
}
