using UnityEngine;
using Player;

[RequireComponent(typeof(PlayerInputs))]
[RequireComponent(typeof(PlayerLocomotions))]
[RequireComponent(typeof(PlayerAnimations))]
[RequireComponent(typeof(PlayerHealth))]
[RequireComponent(typeof(PlayerWeapons))]
[RequireComponent(typeof(PlayerPresenter))]
[RequireComponent(typeof(PlayerUI))]
public class PlayerMediator : MonoBehaviour
{
    public PlayerLocomotions Locomotions { get; private set; }
    public PlayerAnimations Animations { get; private set; }
    public PlayerWeapons Weapons { get; private set; }
    public PlayerHealth Health { get; private set; }
    public PlayerInputs Inputs { get; private set; }
    public PlayerEvents Events { get; private set; }
    public PlayerPresenter PlayerPresenter { get; private set; }
    public PlayerUI PlayerUI { get; private set; }

    private BaseState _currentState;

    public IdleState IdleState { get; private set; }
    public MoveState MoveState { get; private set; }
    public JumpState JumpState { get; private set; }
    public DodgeState DodgeState { get; private set; }
    public LandState LandState { get; private set; }
    public ShotState ShotState { get; private set; }
    public SwingState SwingState { get; private set; }
    public SwapState SwapState { get; private set; }
    public ReloadState ReloadState { get; private set; }
    public ThrowState ThrowState { get; private set; }
    public DieState DieState { get; private set; }

    void Awake()
    {
        Locomotions = GetComponent<PlayerLocomotions>();
        Animations = GetComponent<PlayerAnimations>();
        Health = GetComponent<PlayerHealth>();
        Weapons = GetComponent<PlayerWeapons>();
        Inputs = GetComponent<PlayerInputs>();
        PlayerPresenter = GetComponent<PlayerPresenter>();
        PlayerUI = GetComponent<PlayerUI>();

        Events = new PlayerEvents();
        if (TryGetComponent<PlayerInputs>(out PlayerInputs inputs))
        {
            inputs.Initialize(events: Events);
        }
        Locomotions.Initialize();
        Animations.Initialize(events: Events);
        Health.Initialize(events: Events);
        Weapons.Initialize(events: Events);

        IdleState = new IdleState(this);
        MoveState = new MoveState(this);
        JumpState = new JumpState(this);
        DodgeState = new DodgeState(this);
        LandState = new LandState(this);
        ShotState = new ShotState(this);
        SwingState = new SwingState(this);
        SwapState = new SwapState(this);
        ReloadState = new ReloadState(this);
        ThrowState = new ThrowState(this);
        DieState = new DieState(this);
    }

    void OnEnable()
    {
        // input
        Events.OnJump += HandleJump;
        Events.OnDodge += HandleDodge;
        Events.OnAttack += HandleAttack;
        Events.OnReload += HandleReload;
        Events.OnWeaponSwap += HandleWeaponSwap;

        // animation
        Events.OnAnimationCommit += HandleAnimationCommit;
        Events.OnAnimationFinished += HandleAnimationFinished;

        // health
        Events.OnDie += HandleDie;
    }

    void OnDisable()
    {
        // input
        Events.OnJump -= HandleJump;
        Events.OnDodge -= HandleDodge;
        Events.OnAttack -= HandleAttack;
        Events.OnWeaponSwap -= HandleWeaponSwap;
        Events.OnReload -= HandleReload;
        // animation
        Events.OnAnimationCommit -= HandleAnimationCommit;
        Events.OnAnimationFinished -= HandleAnimationFinished;
        // health
        Events.OnDie -= HandleDie;
    }

    void Start()
    {
        ChangeState(IdleState);
    }

    void Update()
    {
        Locomotions.SetMousePosition(Inputs.MousePosition);
        Locomotions.SetMuzzleHeight(Weapons.MuzzleHeight);
        Locomotions.Tick();
        _currentState.Tick();
    }

    public void AttachUI(Transform overlayCanvas, Transform worldCanvas)
    {
        PlayerUI.Initialize(overlayCanvas, worldCanvas);
        PlayerPresenter.Initialize(Health, Weapons, Events, PlayerUI);
    }

    public void ChangeState(BaseState newState)
    {
        if (_currentState == DieState) return;

        _currentState?.Exit();
        Animations.ResetAllTriggers();
        _currentState = newState;
        _currentState?.Enter();
    }

    // input
    private void HandleJump() => _currentState?.HandleJump();
    private void HandleDodge() => _currentState?.HandleDodge();
    private void HandleReload() => _currentState?.HandleReload();
    private void HandleAttack() => _currentState?.HandleAttack();
    private void HandleWeaponSwap(int index) => _currentState?.HandleWeaponSwap(index);

    // animation
    private void HandleAnimationCommit() => _currentState?.HandleAnimationCommit();
    private void HandleAnimationFinished() => _currentState?.HandleAnimationFinished();

    // health
    private void HandleDie() => ChangeState(DieState);
}