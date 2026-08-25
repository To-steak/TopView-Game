using System;

public class EnemyEvents
{
    // Health
    public event Action OnDie;
    public event Action<float, float> OnHealthChanged;

    public void CallbackOnDie() => OnDie?.Invoke();
    public void CallbackOnHealthChanged(float current, float max) => OnHealthChanged?.Invoke(current, max);

    // Animation
    public event Action OnAnimationFinished;
    public event Action OnAttackCommit;

    public void CallbackOnAnimationFinished() => OnAnimationFinished?.Invoke();
    public void CallbackOnAttackCommit() => OnAttackCommit?.Invoke();
}