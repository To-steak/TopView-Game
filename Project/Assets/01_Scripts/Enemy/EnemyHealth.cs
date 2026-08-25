using UnityEngine;

public class EnemyHealth : MonoBehaviour, IDamageable
{
    [SerializeField] private EnemyHealthSO healthSO;

    public float CurrentHealth { get; private set; }
    public float MaxHealth { get; private set; }
    public float AttackDamage { get; private set; }
    public int Score { get; private set; }
    public int Level { get; private set; }

    private bool _isDie = false;
    private EnemyEvents _events;
    private float _incremental;

    public void Initialize(EnemyEvents events)
    {
        _events = events;
        _incremental = healthSO.Incremental;
    }

    public void PrepareSpawn(int level)
    {
        Level = level;
        Score = healthSO.Score;
        AttackDamage = healthSO.BasicAttackDamage + Level * _incremental;
        MaxHealth = healthSO.BasicMaxHealth + Level * _incremental;
        CurrentHealth = MaxHealth;
        _isDie = false;

        _events?.CallbackOnHealthChanged(CurrentHealth, MaxHealth);
    }

    public void TakeDamage(float amount)
    {
        if (_isDie) return;

        CurrentHealth = Mathf.Max(0, CurrentHealth - amount);
        _events.CallbackOnHealthChanged(CurrentHealth, MaxHealth);

        if (CurrentHealth <= 0)
        {
            _isDie = true;
            _events.CallbackOnDie();
        }
    }
}
