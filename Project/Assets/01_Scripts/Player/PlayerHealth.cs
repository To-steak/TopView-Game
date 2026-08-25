using System;
using UnityEngine;

public class PlayerHealth : MonoBehaviour, IDamageable
{
    [SerializeField] private PlayerHealthSO healthSO;

    public float RunCost => healthSO.RunCost;
    public float DodgeCost => healthSO.DodgeCost;
    public float JumpCost => healthSO.JumpCost;

    public float CurrentHealth { get; private set; }
    public float CurrentMana { get; private set; }
    public float MaxHealth { get; private set; }
    public float MaxMana { get; private set; }

    private PlayerEvents _events;
    private float _recoveryTimer;
    private bool _isDie;

    void Update()
    {
        if (_recoveryTimer < healthSO.RecoveryDelay)
        {
            _recoveryTimer += Time.deltaTime;
        }

        if (CurrentMana < healthSO.MaxMana && _recoveryTimer >= healthSO.RecoveryDelay)
        {
            CurrentMana += healthSO.RecoveryMana * Time.deltaTime;
            CurrentMana = Mathf.Min(CurrentMana, MaxMana);
            _events.CallbackOnManaChanged(CurrentMana, MaxMana);
        }
    }

    public void Initialize(PlayerEvents events)
    {
        _events = events;
        _isDie = false;
        
        MaxHealth = healthSO.MaxHealth;
        MaxMana = healthSO.MaxMana;

        CurrentHealth = MaxHealth;
        CurrentMana = MaxMana;

        _recoveryTimer = healthSO.RecoveryDelay;
    }

    public void UseMana(float amount)
    {
        CurrentMana -= amount;
        if (CurrentMana < 0)
        {
            CurrentMana = 0;
        }
        _recoveryTimer = 0;

        _events.CallbackOnManaChanged(CurrentMana, MaxMana);
    }

    public void TakeDamage(float amount)
    {
        if (_isDie)
        {
            return;
        }

        CurrentHealth -= amount;

        if (CurrentHealth <= 0)
        {
            CurrentHealth = 0;
            _isDie = true;
            _events.CallbackOnDie();
        }

        _events.CallbackOnHealthChanged(CurrentHealth, MaxHealth);
    }

    public bool HasEnoughMana(float amount)
    {
        return CurrentMana >= amount;
    }
}
