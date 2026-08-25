using UnityEngine;

public class Bullet : MonoBehaviour
{
    private float _damage;
    private float _speed;
    private LayerMask _layer;
    private float _timer;
    private TrailRenderer _trailRenderer;
    private ParticleSystem _particlePrefab;
    private const float MAX_LIFE_TIME = 5f;

    public void Initialize(ParticleSystem particle, float speed, LayerMask layer, float pitch = 0f)
    {
        _speed = speed;
        _layer = layer;
        _particlePrefab = particle;
        _trailRenderer = GetComponent<TrailRenderer>();

        var rotator = GetComponentInChildren<FBXRotator>();
        if (rotator != null)
        {
            rotator.PitchSpeed = pitch;
        }
    }
    
    public void SetDamage(float damage)
    {
        _damage = damage;
    }

    void OnEnable()
    {
        _timer = 0;
        if (_trailRenderer != null)
        {
            _trailRenderer.Clear();
        }
    }

    void Update()
    {
        transform.Translate(Vector3.forward * _speed * Time.deltaTime);

        _timer += Time.deltaTime;
        if (_timer >= MAX_LIFE_TIME)
        {
            gameObject.SetActive(false);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if ((_layer.value & (1 << other.gameObject.layer)) > 0)
        {
            if (other.TryGetComponent<IDamageable>(out IDamageable component))
            {
                component.TakeDamage(_damage);
                if (_particlePrefab != null)
                {
                    Instantiate(_particlePrefab, transform.position, Quaternion.identity);
                }
                gameObject.SetActive(false);
            }
        }
    }
}
