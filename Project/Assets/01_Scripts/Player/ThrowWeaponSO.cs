using UnityEngine;
using Player;

[CreateAssetMenu(fileName = "New Throw Weapon", menuName = "Scriptable Objects/PlayerWeaponsSO/Throw")]
public class ThrowWeaponSO : PlayerWeaponSO<ThrowWeaponInstance>
{
    public GameObject GrenadePrefab;
    public ParticleSystem ParticlePrefab;
    public int ReserveAmmo;
    public int PoolSize;
    public float GrenadeSpeed;
    public float UpwardRatio;
    public float ExplosionRadius;
    public float ExplosionDelay;
    public LayerMask TargetLayer;

    public override ThrowWeaponInstance CreateInstance() => new ThrowWeaponInstance(this, ReserveAmmo);

    public override void Initialize(ThrowWeaponInstance instance, Transform weaponSocket)
    {
        if (WeaponPrefab != null)
        {
            GameObject prefab = Instantiate(WeaponPrefab, weaponSocket);
            prefab.transform.localPosition = Vector3.zero;
            prefab.transform.localRotation = Quaternion.identity;
            prefab.SetActive(false);

            instance.SpawnedPrefab = prefab;
        }

        if (GrenadePrefab != null)
        {
            instance.Container = new GameObject($"{WeaponName} grenade pool");

            for (int i = 0; i < PoolSize; i++)
            {
                var grenade = Instantiate(GrenadePrefab, instance.Container.transform);
                grenade.SetActive(false);
                if (grenade.TryGetComponent<Grenade>(out var component))
                {
                    component.Initialize(ParticlePrefab, Damage, ExplosionRadius, ExplosionDelay, TargetLayer);
                    instance.GrenadePool.Add(component);
                }
            }
        }
    }

    public override BaseState GetAttackState(PlayerMediator mediator, ThrowWeaponInstance instance)
    {
        if (instance.ReserveAmmo <= 0) return null;
        return mediator.ThrowState;
    }

    public override void BeginAttack(ThrowWeaponInstance instance, Transform playerTransform, Transform muzzle)
    {
        if (instance.ReserveAmmo <= 0) return;


        Grenade grenade = null;
        foreach (var item in instance.GrenadePool)
        {
            if (!item.gameObject.activeInHierarchy)
            {
                grenade = item;
                break;
            }
        }

        if (grenade == null)
        {
            GameObject newGrenade = Instantiate(GrenadePrefab, instance.Container.transform);
            if (newGrenade.TryGetComponent<Grenade>(out var component))
            {
                component.Initialize(ParticlePrefab, Damage, ExplosionRadius, ExplosionDelay, TargetLayer);
                instance.GrenadePool.Add(component);
                grenade = component;
            }
        }

        if (grenade != null)
        {
            instance.ReserveAmmo--;

            grenade.transform.position = muzzle.position;
            grenade.transform.rotation = playerTransform.rotation;

            Vector3 throwDirection = playerTransform.forward + Vector3.up * UpwardRatio;
            Vector3 throwVelocity = throwDirection.normalized * GrenadeSpeed;
            grenade.Throw(throwVelocity);

            grenade.gameObject.SetActive(true);
        }
    }

    public override void TickAttack(ThrowWeaponInstance instance, Transform playerTransform, Transform muzzle) { }

    public override void EndAttack(ThrowWeaponInstance instance, Transform playerTransform, Transform muzzle) { }

    public override string GetAmmoText(ThrowWeaponInstance instance) => $"{instance.ReserveAmmo}";
}