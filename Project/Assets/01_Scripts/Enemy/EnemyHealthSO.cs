using UnityEngine;

[CreateAssetMenu(fileName = "EnemyHealthSO", menuName = "Scriptable Objects/EnemyHealthSO")]
public class EnemyHealthSO : EnemySO
{
    public float Incremental;
    public float BasicAttackDamage;
    public float BasicMaxHealth;
    public int Score;
}
