using TMPro;
using UnityEngine;

public class GameInfoView : MonoBehaviour
{
    [SerializeField] private TMP_Text timeText;
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private GameObject gameoverPanel;
    [SerializeField] private GameObject gameStartPanel;

    public void UpdateTimeText(float time)
    {
        int minutes = (int)time / 60;
        int seconds = (int)time % 60;
        timeText.text = $"{minutes:00} : {seconds:00}";
    }

    public void UpdateScoreText(int score)
    {
        scoreText.text = score.ToString("N0");
    }

    public void ActiveGameoverPanel()
    {
        gameoverPanel.SetActive(true);
    }

    public void DeactiveGameStartPanel()
    {
        gameStartPanel.SetActive(false);
    }
}
