using System;

public class PlayerEvents
{
    // Input
    public event Action OnJump;
    public event Action OnDodge;
    public event Action OnAttack;
    public event Action<int> OnWeaponSwap;
    public event Action OnReload;

    public void CallbackOnJump() => OnJump?.Invoke();
    public void CallbackOnDodge() => OnDodge?.Invoke();
    public void CallbackOnAttack() => OnAttack?.Invoke();
    public void CallbackOnWeaponSwap(int index) => OnWeaponSwap?.Invoke(index);
    public void CallbackOnReload() => OnReload?.Invoke();

    // Animation
    public event Action OnAnimationFinished;
    public event Action OnAnimationCommit;

    public void CallbackOnAnimationFinished() => OnAnimationFinished?.Invoke();
    public void CallbackOnAnimationCommitted() => OnAnimationCommit?.Invoke();

    // Health
    public event Action OnDie;
    public event Action<float, float> OnHealthChanged;
    public event Action<float, float> OnManaChanged;

    public void CallbackOnDie() => OnDie?.Invoke();
    public void CallbackOnHealthChanged(float current, float max) => OnHealthChanged?.Invoke(current, max);
    public void CallbackOnManaChanged(float current, float max) => OnManaChanged?.Invoke(current, max);

    // Weapon
    public event Action<string> OnAmmoChanged;

    public void CallbackOnAmmoChanged(string ammoText) => OnAmmoChanged?.Invoke(ammoText);
}
