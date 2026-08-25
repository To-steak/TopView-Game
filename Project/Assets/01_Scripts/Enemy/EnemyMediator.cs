using UnityEngine;
using Enemy;
using UnityEngine.Pool;

[RequireComponent(typeof(EnemyHealth))]
[RequireComponent(typeof(EnemyAnimations))]
[RequireComponent(typeof(EnemyAI))]
[RequireComponent(typeof(EnemyAttacks))]
[RequireComponent(typeof(EnemyPresenter))]
public class EnemyMediator : MonoBehaviour
{
    public EnemyHealth Health { get; private set; }
    public EnemyAnimations Animations { get; private set; }
    public EnemyAI EnemyAI { get; private set; }
    public EnemyAttacks Attack { get; private set; }
    public EnemyEvents Events { get; private set; }
    public EnemyPresenter Presenter { get; private set; }

    public IdleState IdleState { get; private set; }
    public WalkState WalkState { get; private set; }
    public AttackState AttackState { get; private set; }
    public DieState DieState { get; private set; }

    private BaseState _currentState;
    private IObjectPool<EnemyMediator> _pool;

    void Awake()
    {
        Health = GetComponent<EnemyHealth>();
        Animations = GetComponent<EnemyAnimations>();
        Attack = GetComponent<EnemyAttacks>();
        Presenter = GetComponent<EnemyPresenter>();
        EnemyAI = GetComponent<EnemyAI>();

        Events = new EnemyEvents();
        Attack.Initialize();
        Animations.Initialize(events: Events);
        Health.Initialize(events: Events);
        Presenter.Initialize(health: Health, events: Events);
        EnemyAI.Initialize();

        IdleState = new IdleState(this);
        WalkState = new WalkState(this);
        AttackState = new AttackState(this);
        DieState = new DieState(this);
    }

    void OnEnable()
    {
        Events.OnAnimationFinished += HandleAnimationFinished;
        Events.OnAttackCommit += HandleAttackCommit;
        Events.OnDie += HandleDie;

        if (IdleState != null)
        {
            _currentState = IdleState;
            _currentState.Enter();
        }
    }

    void OnDisable()
    {
        Events.OnAnimationFinished -= HandleAnimationFinished;
        Events.OnAttackCommit -= HandleAttackCommit;
        Events.OnDie -= HandleDie;
    }

    void Update()
    {
        _currentState?.Tick();
    }

    public void SetPool(IObjectPool<EnemyMediator> pool)
    {
        _pool = pool;
    }

    public void ReturnPool()
    {
        if (_pool != null)
        {
            _pool.Release(this);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void ChangeState(BaseState newState)
    {
        if (_currentState == DieState)
        {
            return;
        }

        _currentState?.Exit();
        _currentState = newState;
        _currentState?.Enter();
    }

    public void HandleAttackCommit() => _currentState?.HandleAttackCommit();
    public void HandleAnimationFinished() => _currentState?.HandleAnimationFinished();
    public void HandleDie() => ChangeState(DieState);
}
