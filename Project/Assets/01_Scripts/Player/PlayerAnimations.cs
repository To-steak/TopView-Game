using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimations : MonoBehaviour, IAnimationEventReceiver
{
    private readonly int speedHash = Animator.StringToHash("MovementSpeed");
    private readonly int jumpHash = Animator.StringToHash("IsJump");
    private readonly int dodgeHash = Animator.StringToHash("Dodge");
    private readonly int shotHash = Animator.StringToHash("Shot");
    private readonly int swingHash = Animator.StringToHash("Swing");
    private readonly int swapHash = Animator.StringToHash("Swap");
    private readonly int reloadHash = Animator.StringToHash("Reload");
    private readonly int throwHash = Animator.StringToHash("Throw");
    private readonly int dieHash = Animator.StringToHash("Die");

    private bool _isRunning = false;
    private Animator _animator;
    private PlayerEvents _events;
    private const float IDLE_SPEED = 0f;
    private const float WALK_SPEED = 1f;
    private const float RUN_SPEED = 2f;
    private int[] _triggerHashes;

    public void Initialize(PlayerEvents events)
    {
        _events = events;
        _animator = GetComponentInChildren<Animator>();
        
        var triggers = new List<int>();
        foreach (var param in _animator.parameters)
        {
            if (param.type == AnimatorControllerParameterType.Trigger)
                triggers.Add(param.nameHash);
        }
        _triggerHashes = triggers.ToArray();
    }

    public void SetMoveAnimation(Vector3 direction)
    {
        if (direction == Vector3.zero)
        {
            _animator.SetFloat(speedHash, IDLE_SPEED);
        }
        else
        {
            _animator.SetFloat(speedHash, _isRunning ? RUN_SPEED : WALK_SPEED);
        }
    }

    public void SetRunAnimation(bool isPerformed)
    {
        _isRunning = isPerformed;

        if (_animator.GetFloat(speedHash) > 0f)
        {
            _animator.SetFloat(speedHash, _isRunning ? 2f : 1f);
        }
    }

    public void SetJump(bool value)
    {
        _animator.SetBool(jumpHash, value);
    }

    public void PlayDodge()
    {
        _animator.SetTrigger(dodgeHash);
    }

    public void PlayShot()
    {
        _animator.SetTrigger(shotHash);
    }

    public void PlaySwing()
    {
        _animator.SetTrigger(swingHash);
    }

    public void PlaySwap()
    {
        _animator.SetTrigger(swapHash);
    }

    public void PlayReload()
    {
        _animator.SetTrigger(reloadHash);
    }

    public void PlayThrow()
    {
        _animator.SetTrigger(throwHash);
    }

    public void PlayDie()
    {

        _animator.SetTrigger(dieHash);
    }

    public void ResetAllTriggers()
    {
        foreach (var hash in _triggerHashes)
        {
            _animator.ResetTrigger(hash);
        }
    }

    public void NotifyAnimationFinished() => _events.CallbackOnAnimationFinished();
    public void NotifyAnimationCommit() => _events.CallbackOnAnimationCommitted();
}