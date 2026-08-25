using UnityEngine;

[CreateAssetMenu(fileName = "PlayerHealthSO", menuName = "Scriptable Objects/PlayerHealthSO")]
public class PlayerHealthSO : PlayerSO
{
    public float MaxHealth;
    public float MaxMana;
    public float RecoveryMana;
    public float RecoveryDelay;
    public float RunCost;
    public float DodgeCost;
    public float JumpCost;
}
