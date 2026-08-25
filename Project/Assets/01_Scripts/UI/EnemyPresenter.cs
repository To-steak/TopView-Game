using UnityEngine;

public class EnemyPresenter : MonoBehaviour
{
    [SerializeField] private EnemyUISO enemyUISO;
    private GaugeWorldView _gaugeWorldView;
    private EnemyHealth _health;
    private EnemyEvents _events;

    public void Initialize(EnemyHealth health, EnemyEvents events)
    {
        _health = health;
        _events = events;
    }

    public void AttachUI(GaugeWorldView view)
    {
        _gaugeWorldView = view;
        _gaugeWorldView.SetTarget(this.transform, enemyUISO.Offset);
        _gaugeWorldView.SetColor(enemyUISO.Color);

        _events.OnHealthChanged += _gaugeWorldView.UpdateGauge;
        _gaugeWorldView.UpdateGauge(_health.CurrentHealth, _health.MaxHealth);
    }

    public GaugeWorldView DetachUI()
    {
        if (_gaugeWorldView != null)
        {
            _events.OnHealthChanged -= _gaugeWorldView.UpdateGauge;
            _gaugeWorldView.SetTarget(null, Vector3.zero);

            GaugeWorldView detachedView = _gaugeWorldView;
            _gaugeWorldView = null;
            return detachedView;
        }
        return null;
    }
}