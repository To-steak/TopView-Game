using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerLocomotions : MonoBehaviour
{
    [SerializeField] private PlayerLocomotionsSO locomotionsSO;

    public bool IsGround { get; private set; }
    public float VerticalVelocity { get; private set; }
    public float InAirSpeed => locomotionsSO.InAirSpeed;
    public float WalkSpeed => locomotionsSO.WalkSpeed;
    public float RunSpeed => locomotionsSO.RunSpeed;

    private CharacterController _controller;
    private Camera _mainCamera;
    private bool _isRotationsLocked = false;
    private float _muzzleHeight;
    private Vector3 _direction;
    private Vector2 _mousePosition;
    private float _currentSpeed;

    public void Initialize()
    {
        if (locomotionsSO == null)
        {
            Debug.LogError("PlayerLocomotions >> locomotionsSO가 없다.");
            enabled = false;
            return;
        }

        if (Camera.main == null)
        {
            Debug.LogError("PlayerLocomotions >> Scene에 MainCamera 태그를 가진 카메라가 없다.");
            enabled = false;
            return;
        }

        _mainCamera = Camera.main;
        _controller = GetComponent<CharacterController>();
        _currentSpeed = locomotionsSO.WalkSpeed;
    }

    public void Tick()
    {
        Vector3 CheckSphere = transform.position + locomotionsSO.GroundCheckOffset;
        IsGround = Physics.CheckSphere(CheckSphere, locomotionsSO.GroundDistance, locomotionsSO.GroundLayer);

        if (IsGround && VerticalVelocity < 0)
        {
            VerticalVelocity = locomotionsSO.Pressure;
        }
        VerticalVelocity += locomotionsSO.Gravity * Time.deltaTime;

        Vector3 moveDirection = GetQuarterDirection(_direction);
        Vector3 movement = moveDirection * _currentSpeed;
        movement.y = VerticalVelocity;
        _controller.Move(movement * Time.deltaTime);

        if (!_isRotationsLocked)
        {
            Ray ray = _mainCamera.ScreenPointToRay(_mousePosition);
            Plane plane = new Plane(Vector3.up, new Vector3(0, _muzzleHeight, 0));

            if (plane.Raycast(ray, out float distance))
            {
                Vector3 hitPoint = ray.GetPoint(distance);
                Vector3 direction = hitPoint - transform.position;
                direction.y = 0;

                if (direction.sqrMagnitude > 0.01f)
                {
                    transform.rotation = Quaternion.LookRotation(direction);
                }
            }
        }
    }

    public void SetDirection(Vector3 direction)
    {
        _direction = direction;
    }

    public void SetMousePosition(Vector2 position)
    {
        _mousePosition = position;
    }

    public void SetRotationLock(bool isLocked)
    {
        _isRotationsLocked = isLocked;
    }

    public void SetSpeed(float speed)
    {
        _currentSpeed = speed;
    }

    public void SetMuzzleHeight(float height)
    {
        _muzzleHeight = height;
    }

    public void DoJump()
    {
        VerticalVelocity = locomotionsSO.JumpPower;
    }

    public void DoDodge()
    {
        _currentSpeed = locomotionsSO.DodgeSpeed;
        transform.rotation = Quaternion.LookRotation(GetQuarterDirection(_direction));
    }

    private Vector3 GetQuarterDirection(Vector3 direction)
    {
        if (_mainCamera == null) return direction;

        Vector3 camForward = _mainCamera.transform.forward;
        Vector3 camRight = _mainCamera.transform.right;

        camForward.y = 0f;
        camRight.y = 0f;

        camForward.Normalize();
        camRight.Normalize();

        return (camForward * direction.z) + (camRight * direction.x);
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (IsGround)
        {
            Gizmos.color = Color.green;
        }
        else
        {
            Gizmos.color = Color.red;
        }
        Gizmos.DrawWireSphere(transform.position + locomotionsSO.GroundCheckOffset, locomotionsSO.GroundDistance);
    }
#endif
}
