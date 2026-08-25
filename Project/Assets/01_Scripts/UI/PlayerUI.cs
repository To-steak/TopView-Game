using UnityEngine;

public class PlayerUI : MonoBehaviour
{
    [SerializeField] private PlayerUISO playerUISO;
    [SerializeField] private HUDView hudPrefab;
    [SerializeField] private GaugeWorldView gaugePrefab;

    private HUDView _hud;
    private GaugeWorldView _gauge;

    public void Initialize(Transform overlayCanvas, Transform worldCanvas)
    {
        if (hudPrefab != null && overlayCanvas != null)
        {
            _hud = Instantiate(hudPrefab, overlayCanvas);
        }

        if (gaugePrefab != null && worldCanvas != null)
        {
            _gauge = Instantiate(gaugePrefab, worldCanvas);
        }
    }

    public void UpdateHealthText(float current, float max) => _hud?.UpdateHealthText(current, max);

    public void UpdateHealthGauge(float current, float max) => _hud?.UpdateHealthGauge(current, max);

    public void UpdateAmmoText(string text) => _hud?.UpdateAmmoText(text);

    public void UpdateManaGauge(float current, float max) => _gauge?.UpdateGauge(current, max);

    public void SetManaGaugeTarget() => _gauge?.SetTarget(gameObject.transform, playerUISO.offset);
}