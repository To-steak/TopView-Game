using System;

public static class GameEvents
{
    public static event Action<int> OnEntityDied;
    public static event Action OnPlayerDied;
    public static event Action OnBossDied;

    public static void CallbackEntityDied(int score)
    {
        OnEntityDied?.Invoke(score);
    }

    public static void CallbackPlayerDied()
    {
        OnPlayerDied?.Invoke();
    }

    public static void CallbackBossDied()
    {
        OnBossDied?.Invoke();
    }
}