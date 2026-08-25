using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Grenade : MonoBehaviour
{
    private float _damage;
    private float _explosionRadius;
    private LayerMask _layer;
    private float _timer;
    private float _explosionTime;
    private ParticleSystem _particlePrefab;
    private Rigidbody _rb;

    public void Initialize(ParticleSystem particlePrefab, float damage, float radius, float explosionTime, LayerMask layer)
    {
        _rb = GetComponent<Rigidbody>();
        _particlePrefab = particlePrefab;
        _damage = damage;
        _explosionRadius = radius;
        _explosionTime = explosionTime;
        _layer = layer;
    }

    public void Throw(Vector3 throwVelocity)
    {
        _rb.linearVelocity = throwVelocity;
    }

    void OnEnable()
    {
        _timer = 0;
    }

    void Update()
    {
        if (!gameObject.activeSelf)
        {
            return;
        }
        
        _timer += Time.deltaTime;

        if (_timer >= _explosionTime)
        {
            Explode();
        }
    }

    private void Explode()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, _explosionRadius, _layer);

        foreach (var hit in hitColliders)
        {
            if (hit.TryGetComponent<IDamageable>(out IDamageable component))
            {
                component.TakeDamage(_damage);
            }
        }

        if (_particlePrefab != null)
        {
            Instantiate(_particlePrefab, transform.position, Quaternion.identity);
        }
        gameObject.SetActive(false);
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _explosionRadius);
    }
#endif
}