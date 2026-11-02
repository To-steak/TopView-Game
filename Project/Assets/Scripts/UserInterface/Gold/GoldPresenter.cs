using UnityEngine;

namespace TopView.UserInterface.Gold
{
    // Runner가 매 프레임 넘기는 골드를 보여 준다.
    public class GoldPresenter : MonoBehaviour
    {
        [SerializeField] private GoldView _view;

        // 같은 값을 매 프레임 다시 쓰지 않도록 마지막으로 보여 준 값을 기억한다
        private int _presentedGold;

        private const int NOT_PRESENTED = -1;

        /// <summary>
        /// 보여 준 값을 처음 상태로 되돌린다. 판을 준비할 때 Runner가 부른다.
        /// </summary>
        public void Initialize()
        {
            _presentedGold = NOT_PRESENTED;
        }

        /// <summary>
        /// 골드가 바뀌었을 때만 View에 보여 준다.
        /// </summary>
        /// <param name="gold">골드</param>
        public void Present(int gold)
        {
            if (gold != _presentedGold)
            {
                _view.SetGold(gold);
                _presentedGold = gold;
            }
        }
    }
}
