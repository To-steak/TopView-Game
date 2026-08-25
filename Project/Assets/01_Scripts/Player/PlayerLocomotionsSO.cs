using UnityEngine;

[CreateAssetMenu(fileName = "PlayerLocomotionsSO", menuName = "Scriptable Objects/PlayerLocomotionsSO")]
public class PlayerLocomotionsSO : PlayerSO
{
    [Header("Movement Settings")]
    public float WalkSpeed;
    public float RunSpeed;
    public float JumpPower;
    public float DodgeSpeed;
    public float InAirSpeed;
    [Header("Ground Checker")]
    public float Gravity;
    public float Pressure;
    public Vector3 GroundCheckOffset;
    public LayerMask GroundLayer;
    public float GroundDistance;
}
