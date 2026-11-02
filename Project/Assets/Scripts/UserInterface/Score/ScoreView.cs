using TMPro;
using UnityEngine;

namespace TopView.UserInterface.Score
{
    // 점수를 보여 준다.
    public class ScoreView : UserInterfaceView
    {
        [SerializeField] private TMP_Text _scoreText;

        /// <summary>
        /// 점수를 천 단위 구분 기호와 함께 보여 준다.
        /// </summary>
        /// <param name="score">점수</param>
        public void SetScore(int score)
        {
            _scoreText.text = score.ToString("N0");
        }
    }
}
