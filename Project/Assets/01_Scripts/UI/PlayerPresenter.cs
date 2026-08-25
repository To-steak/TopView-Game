using UnityEngine;

public class PlayerPresenter : MonoBehaviour
{
    private PlayerHealth _health;
    private PlayerWeapons _weapons;
    private PlayerEvents _events;
    private PlayerUI _ui;

    public void Initialize(PlayerHealth health, PlayerWeapons weapons, PlayerEvents events, PlayerUI ui)
    {
        _health = health;
        _weapons = weapons;
        _events = events;
        _ui = ui;

        _events.OnHealthChanged += _ui.UpdateHealthText;
        _events.OnHealthChanged += _ui.UpdateHealthGauge;
        _events.OnManaChanged += _ui.UpdateManaGauge;
        _events.OnAmmoChanged += _ui.UpdateAmmoText;

        _ui.UpdateHealthText(_health.CurrentHealth, _health.MaxHealth);
        _ui.UpdateHealthGauge(_health.CurrentHealth, _health.MaxHealth);
        _ui.UpdateAmmoText(_weapons.CurrentAmmoText);
        _ui.UpdateManaGauge(_health.CurrentMana, _health.MaxMana);
        _ui.SetManaGaugeTarget();
    }

    void OnDestroy()
    {
        if (_events == null || _ui == null) return;

        _events.OnHealthChanged -= _ui.UpdateHealthText;
        _events.OnHealthChanged -= _ui.UpdateHealthGauge;
        _events.OnManaChanged -= _ui.UpdateManaGauge;
        _events.OnAmmoChanged -= _ui.UpdateAmmoText;
    }
}
