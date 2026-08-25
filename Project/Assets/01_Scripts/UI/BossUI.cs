using UnityEngine;

public class BossUI : MonoBehaviour
{
    [SerializeField] private BossHealthBar bossHealthBar;

    private BossHealthBar _healthBar;

    public void Initialize(Transform overlayCanvas)
    {
        if (bossHealthBar != null && overlayCanvas != null)
        {
            _healthBar = Instantiate(bossHealthBar, overlayCanvas);
        }
    }

    public void UpdateBossHealth(float current, float max) => _healthBar?.UpdateHealthGauge(current, max);
}