using UnityEngine;
using Player;

public class PlayerWeapons : MonoBehaviour
{
    [SerializeField] private PlayerWeaponSO[] weaponSO;
    [SerializeField] private Transform weaponSocket;
    [SerializeField] private Transform muzzle;

    public float MuzzleHeight => muzzle != null ? muzzle.position.y : 0f;
    public string CurrentAmmoText => _currentWeapon != null ? _currentWeapon.GetAmmoText(_instance) : "No Weapon is here";

    private PlayerWeaponSO _currentWeapon => _instance?.WeaponsSO;
    private WeaponInstance[] _weaponInstances;
    private WeaponInstance _instance => (_weaponInstances != null && _weaponInstances.Length > 0) ? _weaponInstances[_currentWeaponIndex] : null;
    private PlayerEvents _events;
    private int _currentWeaponIndex = 0;
    private int _tempWeaponIndex;
    
    public void Initialize(PlayerEvents events)
    {
        _events = events;
        _weaponInstances = new WeaponInstance[weaponSO.Length];

        for (int i = 0; i < weaponSO.Length; i++)
        {
            if (weaponSO[i] != null)
            {
                WeaponInstance instance = weaponSO[i].BaseCreateInstance();

                weaponSO[i].Initialize(instance, weaponSocket);

                _weaponInstances[i] = instance;
            }
        }

        var weapon = _weaponInstances[_currentWeaponIndex];
        if (weapon?.SpawnedPrefab != null)
        {
            weapon.SpawnedPrefab.SetActive(true);
        }
    }

    public bool CanSwap(int index)
    {
        // 현재 무기일 때
        if (index == _currentWeaponIndex)
        {
            return false;
        }

        // 1 ~ 3이 아닌 다른 값이 들어올 때
        if (index < 0 || index >= _weaponInstances.Length)
        {
            return false;
        }

        // 무기가 없을 때
        if (_weaponInstances[index] == null)
        {
            return false;
        }

        return true;
    }

    public void PendingSwap(int index)
    {
        _tempWeaponIndex = index;
    }

    public void ExecuteSwap()
    {
        var oldWeapon = _weaponInstances[_currentWeaponIndex];
        if (oldWeapon?.SpawnedPrefab != null) oldWeapon.SpawnedPrefab.SetActive(false);

        _currentWeaponIndex = _tempWeaponIndex;

        var newWeapon = _weaponInstances[_currentWeaponIndex];
        if (newWeapon != null)
        {
            if (newWeapon?.SpawnedPrefab != null)
            {
                newWeapon.SpawnedPrefab.SetActive(true);
            }
            _events?.CallbackOnAmmoChanged(_currentWeapon.GetAmmoText(newWeapon));
        }
    }

    public BaseState GetAttackState(PlayerMediator mediator)
    {
        if (_currentWeapon == null)
        {
            return null;
        }

        return _currentWeapon.GetAttackState(mediator, _instance);
    }

    public bool CanReload()
    {
        if (_currentWeapon == null)
        {
            return false;
        }

        return _currentWeapon.CanReload(_instance);
    }

    public void Reload()
    {
        if (_currentWeapon != null)
        {
            _currentWeapon.Reload(_instance);
            _events?.CallbackOnAmmoChanged(_currentWeapon.GetAmmoText(_instance));
        }
    }

    public void BeginAttack()
    {
        if (_currentWeapon != null)
        {
            _currentWeapon.BeginAttack(_instance, transform, muzzle);
            _events?.CallbackOnAmmoChanged(_currentWeapon.GetAmmoText(_instance));
        }
    }

    public void TickAttack()
    {
        if (_currentWeapon != null)
        {
            _currentWeapon.TickAttack(_instance, transform, muzzle);
        }
    }

    public void EndAttack()
    {
        if (_currentWeapon != null)
        {
            _currentWeapon.EndAttack(_instance, transform, muzzle);
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (Application.isPlaying)
        {
            _currentWeapon?.OnDrawAttackGizmos(transform, muzzle);
        }
        else
        {
            if (weaponSO != null && weaponSO.Length > _currentWeaponIndex && weaponSO[_currentWeaponIndex] != null)
            {
                weaponSO[_currentWeaponIndex].OnDrawAttackGizmos(transform, muzzle);
            }
        }
    }
#endif
}