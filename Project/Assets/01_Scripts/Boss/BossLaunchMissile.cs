using UnityEngine;

[CreateAssetMenu(fileName = "BossLaunchMissile", menuName = "Scriptable Objects/BossLaunchMissile")]
public class BossLaunchMissile : EnemyAttackSO<BossLaunchMissileInstance>
{
    public Bullet MissilePrefab;
    public float Speed;
    public Vector3 LeftOffset;
    public Vector3 RightOffset;
    public ParticleSystem MissileParticle;
    [Range(0, 3)] public float DamageMultiplier;

    public override void Attack(BossLaunchMissileInstance instance, Transform transform, float damage)
    {
        Vector3 currentOffset = (instance.CurrentShotCount % 2 == 0) ? RightOffset : LeftOffset;
        Vector3 spawnPosition = transform.TransformPoint(currentOffset);

        var bullet = Instantiate(MissilePrefab, spawnPosition, transform.rotation);
        bullet.Initialize(MissileParticle, Speed, Layer);
        bullet.SetDamage(damage * DamageMultiplier);

        instance.CurrentShotCount++;
    }

    public override BossLaunchMissileInstance CreateAttackInstance() => new BossLaunchMissileInstance(this);

}
