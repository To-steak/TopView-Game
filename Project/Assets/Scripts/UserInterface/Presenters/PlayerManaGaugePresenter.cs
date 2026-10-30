using TopView.UserInterface.Views;
using UnityEngine;

namespace TopView.UserInterface.Presenters
{
    // 플레이어 View가 있는 동안 마나 게이지를 플레이어 위에 띄우고, Runner가 매 프레임 넘기는 마나를 보여 준다.
    public class PlayerManaGaugePresenter : MonoBehaviour
    {
        [SerializeField] private GaugeWorldView view;

        private Vector3 _offset;
        private Transform _presentedTarget;
        // 같은 값을 매 프레임 다시 쓰지 않도록 마지막으로 보여 준 값을 기억한다
        private bool _hasPresentedValues;
        private float _presentedMana;
        private float _presentedMaxMana;

        /// <summary>
        /// 게이지를 띄울 거리를 받고, View를 숨긴다. 판을 준비할 때 Runner가 부른다.
        /// </summary>
        /// <param name="offset">플레이어 위치에서 띄울 거리(월드)</param>
        public void Initialize(Vector3 offset)
        {
            _offset = offset;
            _presentedTarget = null;
            _hasPresentedValues = false;
            view.SetTarget(null, Vector3.zero);
            view.gameObject.SetActive(false);
        }

        /// <summary>
        /// 따라갈 플레이어 View가 있으면 게이지를 보이고 마나를 보여 준다. 없으면 숨긴다.
        /// </summary>
        /// <param name="target">플레이어 View의 Transform. 없으면 null</param>
        /// <param name="mana">현재 마나</param>
        /// <param name="maxMana">최대 마나</param>
        public void Present(Transform target, float mana, float maxMana)
        {
            if (target == null)
            {
                if (_presentedTarget != null)
                {
                    view.SetTarget(null, Vector3.zero);
                    view.gameObject.SetActive(false);
                    _presentedTarget = null;
                }
                return;
            }

            if (target != _presentedTarget)
            {
                view.SetTarget(target, _offset);
                view.gameObject.SetActive(true);
                _presentedTarget = target;
            }

            if (!_hasPresentedValues || mana != _presentedMana || maxMana != _presentedMaxMana)
            {
                view.UpdateGauge(mana, maxMana);
                _presentedMana = mana;
                _presentedMaxMana = maxMana;
                _hasPresentedValues = true;
            }
        }
    }
}
