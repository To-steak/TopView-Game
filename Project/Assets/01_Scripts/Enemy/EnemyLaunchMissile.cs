using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyAttackBSO", menuName = "Scriptable Objects/EnemyAttackBSO")]
public class EnemyLaunchMissile : EnemyAttackSO<EnemyLaunchMissileInstance>
{
    public Bullet MissilePrefab;
    public ParticleSystem MissileParticle;
    public float Speed;
    public float Offset;
    public int PoolSize;
    [Range(0, 3)] public float DamageMultiplier;

    [System.NonSerialized] private List<Bullet> _pool = new List<Bullet>();
    [System.NonSerialized] private Transform _container;

    public override EnemyLaunchMissileInstance CreateAttackInstance() => new EnemyLaunchMissileInstance(this);

    public override void Initialize(EnemyLaunchMissileInstance instance)
    {
        if (_container == null)
        {
            _container = new GameObject($"{name} Bullet Pool").transform;
            
            for (int i = 0; i < PoolSize; i++)
            {
                var bullet = Instantiate(MissilePrefab, _container);
                bullet.Initialize(MissileParticle, Speed, Layer);
                bullet.gameObject.SetActive(false);
                _pool.Add(bullet);
            }
        }
    }

    public override void Attack(EnemyLaunchMissileInstance instance, Transform transform, float damage)
    {
        if (transform == null)
        {
            return;
        }

        if (MissilePrefab == null)
        {
            return;
        }

        Bullet bullet = null;
        foreach (var item in _pool)
        {
            if (!item.gameObject.activeInHierarchy)
            {
                bullet = item;
                break;
            }
        }

        Vector3 position = transform.position + (transform.forward * Offset);
        if (bullet == null)
        {
            bullet = Instantiate(MissilePrefab, position, transform.rotation, _container);
            bullet.Initialize(MissileParticle, Speed, Layer);
            _pool.Add(bullet);
        }
        else
        {
            bullet.transform.position = position;
            bullet.transform.rotation = transform.rotation;
            bullet.gameObject.SetActive(true);
        }

        bullet.SetDamage(damage * DamageMultiplier);
    }

    void OnDisable()
    {
        if (_pool != null)
        {
            _pool.Clear();
        }

        _container = null;
    }
}
