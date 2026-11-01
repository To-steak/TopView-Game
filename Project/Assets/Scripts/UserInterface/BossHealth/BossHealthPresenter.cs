using UnityEngine;

namespace TopView.UserInterface.BossHealth
{
    // 보스가 처음 나타나면 BossHealthView를 보이고, 보스가 살아 있는 동안 Runner가 매 프레임 넘기는 체력을 보여 준다.
    public class BossHealthPresenter : MonoBehaviour
    {
        [SerializeField] private BossHealthView view;

        private bool _isShown;
        // 같은 값을 매 프레임 다시 쓰지 않도록 마지막으로 보여 준 값을 기억한다
        private float _presentedHealth;
        private float _presentedMaxHealth;

        /// <summary>
        /// View를 숨긴다. 판을 준비할 때 Runner가 부른다.
        /// </summary>
        public void Initialize()
        {
            view.SetVisible(false);
            _isShown = false;
        }

        /// <summary>
        /// 보스가 있으면 View를 보이고 체력을 보여 준다. 보스가 사라진 뒤에도 View는 마지막 값으로 남는다.
        /// </summary>
        /// <param name="hasBoss">살아 있는 보스가 있는지</param>
        /// <param name="health">현재 체력</param>
        /// <param name="maxHealth">최대 체력</param>
        public void Present(bool hasBoss, float health, float maxHealth)
        {
            if (!hasBoss)
            {
                return;
            }

            if (!_isShown)
            {
                view.SetVisible(true);
                _isShown = true;
                view.SetHealth(health, maxHealth);
                _presentedHealth = health;
                _presentedMaxHealth = maxHealth;
                return;
            }

            if (health != _presentedHealth || maxHealth != _presentedMaxHealth)
            {
                view.SetHealth(health, maxHealth);
                _presentedHealth = health;
                _presentedMaxHealth = maxHealth;
            }
        }
    }
}
