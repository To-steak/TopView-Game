using UnityEngine;
using System.Collections.Generic;

public class WeaponInstance
{
    public PlayerWeaponSO WeaponsSO { get; private set; }
    public GameObject SpawnedPrefab;

    public WeaponInstance(PlayerWeaponSO weaponsSO)
    {
        this.WeaponsSO = weaponsSO;
    }
}

public class RangedWeaponInstance : WeaponInstance
{
    public int CurrentAmmo;
    public int ReserveAmmo;
    public List<Bullet> BulletPool;
    public GameObject Container;

    public RangedWeaponInstance(PlayerWeaponSO weaponsSO, int currentAmmo, int reserveAmmo) : base(weaponsSO)
    {
        CurrentAmmo = currentAmmo;
        ReserveAmmo = reserveAmmo;
        BulletPool = new List<Bullet>();
    }
}

public class MeleeWeaponInstance : WeaponInstance
{
    public Collider[] HitResults;
    public HashSet<Collider> HitTargets;
    public bool IsAttacking;

    public MeleeWeaponInstance(PlayerWeaponSO weaponsSO, int maxTarget) : base(weaponsSO)
    {
        HitResults = new Collider[maxTarget];
        HitTargets = new HashSet<Collider>();
        IsAttacking = false;
    }
}

public class ThrowWeaponInstance : WeaponInstance
{
    public int ReserveAmmo;
    public List<Grenade> GrenadePool;
    public GameObject Container;

    public ThrowWeaponInstance(PlayerWeaponSO weaponsSO, int reserveAmmo) : base(weaponsSO)
    {
        ReserveAmmo = reserveAmmo;
        GrenadePool = new List<Grenade>();
    }
}