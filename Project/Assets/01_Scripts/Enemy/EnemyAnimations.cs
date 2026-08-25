using System.Collections.Generic;
using UnityEngine;

public class EnemyAnimations : MonoBehaviour, IAnimationEventReceiver
{
    private readonly int _walkHash = Animator.StringToHash("isWalk");
    private readonly int _dieHash = Animator.StringToHash("Die");
    private readonly int _tauntHash = Animator.StringToHash("Taunt");
    private int[] _triggerHashes;

    private Animator _animator;
    private EnemyEvents _events;
    private bool _hasWalkParam;

    public void Initialize(EnemyEvents events)
    {
        _events = events;
        _animator = GetComponentInChildren<Animator>();
        _hasWalkParam = false;

        var triggers = new List<int>();
        foreach (var parameter in _animator.parameters)
        {
            if (parameter.nameHash == _walkHash)
            {
                _hasWalkParam = true;
            }

            if (parameter.type == AnimatorControllerParameterType.Trigger)
            {
                triggers.Add(parameter.nameHash);
            }
        }
        _triggerHashes = triggers.ToArray();
    }

    public void PlayIdle()
    {
        foreach (var hash in _triggerHashes)
            _animator.ResetTrigger(hash);

        SetWalk(false); // Boss 는 Walk 가 없음
        _animator.Play("Idle", 0, 0f);
    }

    public void SetWalk(bool isWalk)
    {
        if (_hasWalkParam)
        {
            _animator.SetBool(_walkHash, isWalk);
        }
    }

    public void PlayAttack(int hash)
    {
        _animator.SetTrigger(hash);
    }

    public void PlayDie()
    {
        _animator.SetTrigger(_dieHash);
    }

    public void PlayTaunt()
    {
        _animator.SetTrigger(_tauntHash);
    }

    public void NotifyAnimationFinished() => _events.CallbackOnAnimationFinished();
    public void NotifyAnimationCommit() => _events.CallbackOnAttackCommit();
}
