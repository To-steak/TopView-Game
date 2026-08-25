using UnityEngine;

public class BossPresenter : MonoBehaviour
{
    private EnemyHealth _health;
    private EnemyEvents _events;
    private BossUI _ui;

    public void Initialize(EnemyHealth health, EnemyEvents events, BossUI ui)
    {
        _health = health;
        _events = events;
        _ui = ui;

        _events.OnHealthChanged += _ui.UpdateBossHealth;
    }

    void OnDestroy()
    {
        if (_events == null || _ui == null) return;

        _events.OnHealthChanged -= _ui.UpdateBossHealth;
    }
}