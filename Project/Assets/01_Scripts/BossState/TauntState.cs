using Boss;
using UnityEngine;

public class TauntState : BaseState
{
    public TauntState(BossMediator mediator) : base(mediator) { }
    public override void Enter()
    {
        Vector3 direction = _mediator.Player.position - _mediator.gameObject.transform.position;
        direction.y = 0;
        if (direction.sqrMagnitude > 0.001f)
        {
            _mediator.gameObject.transform.forward = direction.normalized;
        }

        _mediator.Animations.PlayTaunt();
    }

    public override void HandleAnimationFinished()
    {
        _mediator.ChangeState(_mediator.IdleState);
    }
}
