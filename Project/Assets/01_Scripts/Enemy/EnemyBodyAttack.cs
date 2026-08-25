using UnityEngine;

[CreateAssetMenu(fileName = "EnemyAttackASO", menuName = "Scriptable Objects/EnemyAttackASO")]
public class EnemyBodyAttack : EnemyAttackSO<EnemyBodyAttackInstance>
{
    public float Radius;
    public float Offset;
    public int MaxTarget = 1;
    [Range(0, 3)] public float DamageMultiplier;

    public override EnemyBodyAttackInstance CreateAttackInstance() => new EnemyBodyAttackInstance(this, MaxTarget);

    public override void Attack(EnemyBodyAttackInstance instance, Transform transform, float damage)
    {
        if (transform == null)
        {
            return;
        }

        Vector3 position = transform.position + (transform.forward * Offset);
        int count = Physics.OverlapSphereNonAlloc(position, Radius, instance.Hit, Layer);

        for (int i = 0; i < count; i++)
        {
            if (instance.Hit[i].TryGetComponent<IDamageable>(out var component))
            {
                component.TakeDamage(damage * DamageMultiplier);
            }
        }
    }

#if UNITY_EDITOR
    public override void OnDrawAttackGizmos(Transform transform)
    {
        if (transform == null) return;
        Gizmos.color = Color.green;
        Vector3 position = transform.position + (transform.forward * Offset);
        Gizmos.DrawWireSphere(position, Radius);
    }
#endif
}
