using UnityEngine;

public class EnemyAttackInstance
{
    public EnemyAttackSO AttackSO { get; private set; }

    public EnemyAttackInstance(EnemyAttackSO attackSO)
    {
        AttackSO = attackSO;
    }
}

public class EnemyBodyAttackInstance : EnemyAttackInstance
{
    public Collider[] Hit;

    public EnemyBodyAttackInstance(EnemyAttackSO attackSO, int maxTarget) : base(attackSO)
    {
        Hit = new Collider[Mathf.Max(1, maxTarget)];
    }
}

public class EnemyLaunchMissileInstance : EnemyAttackInstance
{
    public EnemyLaunchMissileInstance(EnemyAttackSO attackSO) : base(attackSO)
    {

    }
}

public class BossLaunchMissileInstance : EnemyAttackInstance
{
    public int CurrentShotCount { get; set; }
    public BossLaunchMissileInstance(EnemyAttackSO attackSO) : base(attackSO)
    {
        CurrentShotCount = 0;
    }
}

public class BossRollRockInstance : EnemyAttackInstance
{
    public BossRollRockInstance(EnemyAttackSO attackSO) : base(attackSO)
    {
        
    }
}