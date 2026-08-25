using UnityEngine;

[CreateAssetMenu(fileName = "BossRollRock", menuName = "Scriptable Objects/BossRollRock")]
public class BossRollRock : EnemyAttackSO<BossRollRockInstance>
{
    public Bullet RockPrefab;
    public float Speed;
    public float ForwardOffset;
    public float UpwardOffset;
    public float PitchSpeed;
    public ParticleSystem RockParticle;
    [Range(0, 3)] public float DamageMultiplier;

    public override void Attack(BossRollRockInstance instance, Transform transform, float damage)
    {
        Vector3 position = transform.position + (transform.forward * ForwardOffset) + (transform.up * UpwardOffset);
        var bullet = Instantiate(RockPrefab, position, transform.rotation);
        bullet.Initialize(RockParticle, Speed, Layer, pitch: PitchSpeed);
        bullet.SetDamage(damage * DamageMultiplier);
    }

    public override BossRollRockInstance CreateAttackInstance() => new BossRollRockInstance(this);
}
