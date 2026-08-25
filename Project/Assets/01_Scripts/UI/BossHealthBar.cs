using UnityEngine;
using UnityEngine.UI;

public class BossHealthBar : MonoBehaviour
{
    [SerializeField] private Image hPGauge;

    public void UpdateHealthGauge(float current, float max)
    {
        hPGauge.fillAmount = current / max;
    }
}