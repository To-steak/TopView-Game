using UnityEngine;
using Player;

public abstract class PlayerWeaponSO : ScriptableObject
{
    public GameObject WeaponPrefab;
    public string WeaponName;
    public float Damage;

    public abstract WeaponInstance BaseCreateInstance();
    public abstract void Initialize(WeaponInstance instance, Transform weaponSocket);
    public abstract BaseState GetAttackState(PlayerMediator mediator, WeaponInstance instance);
    public abstract void BeginAttack(WeaponInstance instance, Transform playerTransform, Transform muzzle);
    public abstract void TickAttack(WeaponInstance instance, Transform playerTransform, Transform muzzle);
    public abstract void EndAttack(WeaponInstance instance, Transform playerTransform, Transform muzzle);

    public abstract bool CanReload(WeaponInstance instance);
    public abstract void Reload(WeaponInstance instance);
    public abstract string GetAmmoText(WeaponInstance instance);

#if UNITY_EDITOR
    public virtual void OnDrawAttackGizmos(Transform transform, Transform muzzle) { }
#endif
}

public abstract class PlayerWeaponSO<T> : PlayerWeaponSO where T : WeaponInstance
{
    public override WeaponInstance BaseCreateInstance() => CreateInstance();
    public abstract T CreateInstance();

    public override void Initialize(WeaponInstance instance, Transform weaponSocket) => Initialize((T)instance, weaponSocket);
    public abstract void Initialize(T instance, Transform weaponSocket);

    public override BaseState GetAttackState(PlayerMediator mediator, WeaponInstance instance) => GetAttackState(mediator, (T)instance);
    public abstract BaseState GetAttackState(PlayerMediator mediator, T instance);

    public override void BeginAttack(WeaponInstance instance, Transform playerTransform, Transform muzzle) => BeginAttack((T)instance, playerTransform, muzzle);
    public abstract void BeginAttack(T instance, Transform playerTransform, Transform muzzle);

    public override void TickAttack(WeaponInstance instance, Transform playerTransform, Transform muzzle) => TickAttack((T)instance, playerTransform, muzzle);
    public abstract void TickAttack(T instance, Transform playerTransform, Transform muzzle);

    public override void EndAttack(WeaponInstance instance, Transform playerTransform, Transform muzzle) => EndAttack((T)instance, playerTransform, muzzle);
    public abstract void EndAttack(T instance, Transform playerTransform, Transform muzzle);

    public override bool CanReload(WeaponInstance instance) => CanReload((T)instance);
    public virtual bool CanReload(T instance) => false;

    public override void Reload(WeaponInstance instance) => Reload((T)instance);
    public virtual void Reload(T instance) { }

    public override string GetAmmoText(WeaponInstance instance) => GetAmmoText((T)instance);
    public abstract string GetAmmoText(T instance);
}