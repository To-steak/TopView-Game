using UnityEngine;
using Player;

[CreateAssetMenu(fileName = "New Ranged Weapon", menuName = "Scriptable Objects/PlayerWeaponsSO/Ranged")]
public class RangedWeaponSO : PlayerWeaponSO<RangedWeaponInstance>
{

    public int MaxAmmo;
    public int ReserveAmmo;
    public GameObject BulletPrefab;
    public ParticleSystem BulletParticle;
    public int PoolSize;
    public float BulletSpeed;
    public LayerMask TargetLayer;

    public override RangedWeaponInstance CreateInstance() => new RangedWeaponInstance(this, MaxAmmo, ReserveAmmo);

    public override void Initialize(RangedWeaponInstance instance, Transform weaponSocket)
    {
        if (WeaponPrefab != null)
        {
            GameObject prefab = Instantiate(WeaponPrefab, weaponSocket);
            prefab.transform.localPosition = Vector3.zero;
            prefab.transform.localRotation = Quaternion.identity;
            prefab.SetActive(false);

            instance.SpawnedPrefab = prefab;
        }

        if (BulletPrefab != null)
        {
            instance.Container = new GameObject($"{WeaponName} Bullet Pool");

            for (int i = 0; i < PoolSize; i++)
            {
                GameObject bullet = Instantiate(BulletPrefab, instance.Container.transform);
                bullet.SetActive(false);

                if (bullet.TryGetComponent<Bullet>(out Bullet component))
                {
                    component.Initialize(BulletParticle, BulletSpeed, TargetLayer);
                    instance.BulletPool.Add(component);
                }
            }
        }
    }

    public override BaseState GetAttackState(PlayerMediator mediator, RangedWeaponInstance instance)
    {
        if (instance.CurrentAmmo <= 0)
        {
            return null;
        }

        return mediator.ShotState;
    }

    public override void BeginAttack(RangedWeaponInstance instance, Transform playerTransform, Transform muzzle)
    {
        if (instance.CurrentAmmo <= 0) return;

        Bullet bullet = null;
        foreach (var item in instance.BulletPool)
        {
            if (!item.gameObject.activeInHierarchy)
            {
                bullet = item;
                break;
            }
        }

        if (bullet == null)
        {
            GameObject newBulletObj = Instantiate(BulletPrefab, instance.Container.transform);
            if (newBulletObj.TryGetComponent<Bullet>(out Bullet bulletComponent))
            {
                bulletComponent.Initialize(BulletParticle, BulletSpeed, TargetLayer);
                instance.BulletPool.Add(bulletComponent);
                bullet = bulletComponent;
            }
        }

        if (bullet != null)
        {
            instance.CurrentAmmo--;
            bullet.SetDamage(Damage);
            bullet.transform.position = muzzle.position;
            bullet.transform.rotation = playerTransform.rotation;
            bullet.gameObject.SetActive(true);
        }
    }

    public override void TickAttack(RangedWeaponInstance instance, Transform playerTransform, Transform muzzle) { }

    public override void EndAttack(RangedWeaponInstance instance, Transform playerTransform, Transform muzzle) { }

    public override bool CanReload(RangedWeaponInstance instance)
    {
        return instance.CurrentAmmo < MaxAmmo && instance.ReserveAmmo > 0;
    }

    public override void Reload(RangedWeaponInstance instance)
    {
        int reqAmmo = MaxAmmo - instance.CurrentAmmo;
        int loadAmmo = Mathf.Min(reqAmmo, instance.ReserveAmmo);

        instance.CurrentAmmo += loadAmmo;
        instance.ReserveAmmo -= loadAmmo;
    }

    public override string GetAmmoText(RangedWeaponInstance instance)
    {
        return $"{instance.CurrentAmmo} / {instance.ReserveAmmo}";
    }
}