using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputs : MonoBehaviour
{
    public Vector3 MoveInput { get; private set; }
    public bool IsRunning { get; private set; }
    public Vector2 MousePosition => Mouse.current != null ? Mouse.current.position.ReadValue() : Vector2.zero;

    private Player_Actions _actions;
    private PlayerEvents _events;

    void Awake()
    {
        _actions = new Player_Actions();
    }

    void OnEnable()
    {
        _actions.Global.Enable();

        _actions.Global.Move.performed += OnMove;
        _actions.Global.Move.canceled += OnMove;

        _actions.Global.Run.performed += OnRun;
        _actions.Global.Run.canceled += OnRun;

        _actions.Global.Jump.performed += OnJump;

        _actions.Global.Dodge.performed += OnDodge;

        _actions.Global.Attack.performed += OnAttack;

        _actions.Global.Swap.performed += OnSwap;

        _actions.Global.Reload.performed += OnReload;
    }

    void OnDisable()
    {
        _actions.Global.Move.performed -= OnMove;
        _actions.Global.Move.canceled -= OnMove;

        _actions.Global.Run.performed -= OnRun;
        _actions.Global.Run.canceled -= OnRun;

        _actions.Global.Jump.performed -= OnJump;

        _actions.Global.Dodge.performed -= OnDodge;

        _actions.Global.Attack.performed -= OnAttack;

        _actions.Global.Swap.performed -= OnSwap;

        _actions.Global.Reload.performed -= OnReload;

        _actions.Global.Disable();
    }

    public void Initialize(PlayerEvents events)
    {
        _events = events;
    }

    private void OnMove(InputAction.CallbackContext context)
    {
        Vector2 input = context.ReadValue<Vector2>();
        MoveInput = new Vector3(input.x, 0, input.y);
    }

    private void OnRun(InputAction.CallbackContext context)
    {
        IsRunning = context.ReadValueAsButton();
    }

    private void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed) _events.CallbackOnJump();
    }

    private void OnDodge(InputAction.CallbackContext context)
    {
        if (context.performed) _events.CallbackOnDodge();
    }

    private void OnAttack(InputAction.CallbackContext context)
    {
        if (context.performed) _events.CallbackOnAttack();
    }

    private void OnSwap(InputAction.CallbackContext context)
    {
        if (context.performed) _events.CallbackOnWeaponSwap((int)context.ReadValue<float>() - 1);
    }

    private void OnReload(InputAction.CallbackContext context)
    {
        if (context.performed) _events.CallbackOnReload();
    }
}
