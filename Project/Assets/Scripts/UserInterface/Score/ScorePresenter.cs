using UnityEngine;

namespace TopView.UserInterface.Score
{
    // Runner가 매 프레임 넘기는 점수를 보여 준다.
    public class ScorePresenter : MonoBehaviour
    {
        [SerializeField] private ScoreView _view;

        // 같은 값을 매 프레임 다시 쓰지 않도록 마지막으로 보여 준 값을 기억한다
        private int _presentedScore;

        private const int NOT_PRESENTED = -1;

        /// <summary>
        /// 보여 준 값을 처음 상태로 되돌린다. 판을 준비할 때 Runner가 부른다.
        /// </summary>
        public void Initialize()
        {
            _presentedScore = NOT_PRESENTED;
        }

        /// <summary>
        /// 점수가 바뀌었을 때만 View에 보여 준다.
        /// </summary>
        /// <param name="score">점수</param>
        public void Present(int score)
        {
            if (score != _presentedScore)
            {
                _view.SetScore(score);
                _presentedScore = score;
            }
        }
    }
}
