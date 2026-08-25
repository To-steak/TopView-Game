using UnityEngine;

public abstract class EnemyAttackSO : ScriptableObject
{
    public LayerMask Layer;
    public string AnimationParameter;
    public int AnimationHash { get; private set; }

    void OnEnable()
    {
        AnimationHash = Animator.StringToHash(AnimationParameter);
    }

    public abstract EnemyAttackInstance CreateInstance();
    public virtual void Initialize(EnemyAttackInstance instance) { }
    public abstract void Attack(EnemyAttackInstance instance, Transform transform, float damage);
#if UNITY_EDITOR
    public virtual void OnDrawAttackGizmos(Transform transform) { }
#endif
}

public abstract class EnemyAttackSO<T> : EnemyAttackSO where T : EnemyAttackInstance
{
    public override void Initialize(EnemyAttackInstance instance) => Initialize((T)instance);
    public virtual void Initialize(T instance) { }

    public override void Attack(EnemyAttackInstance instance, Transform transform, float damage) => Attack((T)instance, transform, damage);
    public abstract void Attack(T instance, Transform transform, float damage);

    public override EnemyAttackInstance CreateInstance() => CreateAttackInstance();
    public abstract T CreateAttackInstance();
}
