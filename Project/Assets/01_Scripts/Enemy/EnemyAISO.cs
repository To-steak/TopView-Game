using UnityEngine;

[CreateAssetMenu(fileName = "EnemyAISO", menuName = "Scriptable Objects/EnemyAISO")]
public class EnemyAISO : EnemySO
{
    public float DetectionRadius;
    public LayerMask DetectionLayer;

    public float NextStepTime;

    public float AttackDistance;
    public float AttackRotationSpeed;
    public float Speed;

#if UNITY_EDITOR
    public override void OnDrawAttackGizmos(Transform transform)
    {
        if (transform == null) return;

        Gizmos.color = Color.red;
        Vector3 position = transform.position;
        Gizmos.DrawWireSphere(position, AttackDistance);
    }
#endif
}
