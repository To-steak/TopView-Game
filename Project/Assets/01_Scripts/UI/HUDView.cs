using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HUDView : MonoBehaviour
{
    [SerializeField] private TMP_Text healthText;
    [SerializeField] private TMP_Text ammoText;
    [SerializeField] private Image hPGauge;

    public void UpdateHealthText(float current, float max)
    {
        healthText.text = $"{current} / {max}";
    }

    public void UpdateHealthGauge(float current, float max)
    {
        hPGauge.fillAmount = current / max;
    }

    public void UpdateAmmoText(string text)
    {
        ammoText.text = text;
    }
}
