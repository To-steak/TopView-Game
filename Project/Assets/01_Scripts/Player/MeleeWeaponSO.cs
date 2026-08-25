using UnityEngine;
using Player;

[CreateAssetMenu(fileName = "New Melee", menuName = "Scriptable Objects/PlayerWeaponsSO/Melee")]
public class MeleeWeaponSO : PlayerWeaponSO<MeleeWeaponInstance>
{
    public int MaxTarget;
    public float AttackRadius;
    public LayerMask HitLayer;
    public float AttackOrigin;

    public override MeleeWeaponInstance CreateInstance() => new MeleeWeaponInstance(this, MaxTarget);

    public override void Initialize(MeleeWeaponInstance instance, Transform weaponSocket)
    {
        if (WeaponPrefab != null)
        {
            GameObject prefab = Instantiate(WeaponPrefab, weaponSocket);
            prefab.transform.localPosition = Vector3.zero;
            prefab.transform.localRotation = Quaternion.identity;
            prefab.SetActive(false);

            instance.SpawnedPrefab = prefab;
        }
    }

    public override BaseState GetAttackState(PlayerMediator mediator, MeleeWeaponInstance instance) => mediator.SwingState;

    public override void BeginAttack(MeleeWeaponInstance instance, Transform playerTransform, Transform muzzle)
    {
        instance.IsAttacking = true;
        instance.HitTargets.Clear();
    }

    public override void TickAttack(MeleeWeaponInstance instance, Transform playerTransform, Transform muzzle)
    {
        if (!instance.IsAttacking)
        {
            return;
        }

        int count = Physics.OverlapSphereNonAlloc(playerTransform.position + playerTransform.forward * AttackOrigin, AttackRadius, instance.HitResults, HitLayer);
        for (int i = 0; i < count; i++)
        {
            Collider hitResult = instance.HitResults[i];

            if (instance.HitTargets.Add(hitResult))
            {
                if (hitResult.TryGetComponent<IDamageable>(out IDamageable component))
                {
                    component.TakeDamage(Damage);
                }
            }
        }
    }

    public override void EndAttack(MeleeWeaponInstance instance, Transform playerTransform, Transform muzzle) => instance.IsAttacking = false;
    public override string GetAmmoText(MeleeWeaponInstance instance) => "-";

#if UNITY_EDITOR
    public override void OnDrawAttackGizmos(Transform transform, Transform muzzle)
    {
        if (transform == null) return;
        Gizmos.color = Color.violet;
        Gizmos.DrawWireSphere(transform.position + transform.forward * AttackOrigin, AttackRadius);
    }
#endif
}