using Boss;
using UnityEngine;

[RequireComponent(typeof(EnemyHealth))]
[RequireComponent(typeof(EnemyAnimations))]
[RequireComponent(typeof(EnemyAttacks))]
[RequireComponent(typeof(BossUI))]
[RequireComponent(typeof(BossPresenter))]
public class BossMediator : MonoBehaviour
{
    [SerializeField] private float rotationSpeed = 5f;
    [SerializeField] private float attackEntryTime;

    public EnemyHealth Health { get; private set; }
    public EnemyAnimations Animations { get; private set; }
    public EnemyAttacks Attack { get; private set; }
    public EnemyEvents Events { get; private set; }

    public IdleState IdleState { get; private set; }
    public AttackState AttackState { get; private set; }
    public DieState DieState { get; private set; }
    public TauntState TauntState { get; private set; }
    public Transform Player { get; private set; }
    public BossUI BossUI { get; private set; }
    public BossPresenter BossPresenter { get; private set; }

    public float AttackEntryTime => attackEntryTime;
    public float RotationSpeed => rotationSpeed;

    private BaseState _currentState;

    void Awake()
    {
        Health = GetComponent<EnemyHealth>();
        Animations = GetComponent<EnemyAnimations>();
        Attack = GetComponent<EnemyAttacks>();
        BossUI = GetComponent<BossUI>();
        BossPresenter = GetComponent<BossPresenter>();

        Events = new EnemyEvents();

        Attack.Initialize();
        Animations.Initialize(events: Events);
        Health.Initialize(events: Events);

        IdleState = new IdleState(this);
        AttackState = new AttackState(this);
        DieState = new DieState(this);
        TauntState = new TauntState(this);
    }

    void OnEnable()
    {
        Events.OnAnimationFinished += HandleAnimationFinished;
        Events.OnAttackCommit += HandleAttackCommit;
        Events.OnDie += HandleDie;
    }

    void OnDisable()
    {
        Events.OnAnimationFinished -= HandleAnimationFinished;
        Events.OnAttackCommit -= HandleAttackCommit;
        Events.OnDie -= HandleDie;
    }

    void Update() => _currentState?.Tick();

    public void Initialize(Transform player, int level)
    {
        Player = player;
        Health.PrepareSpawn(level);
        ChangeState(TauntState);
    }

    public void AttachUI(Transform overlayCanvas)
    {
        BossUI.Initialize(overlayCanvas);
        BossPresenter.Initialize(Health, Events, BossUI);
    }

    public void ChangeState(BaseState newState)
    {
        if (_currentState == DieState) return;
        _currentState?.Exit();
        _currentState = newState;
        _currentState?.Enter();
    }

    private void HandleAttackCommit() => _currentState?.HandleAttackCommit();
    private void HandleAnimationFinished() => _currentState?.HandleAnimationFinished();
    private void HandleDie()
    {
        ChangeState(DieState);
    }
}
