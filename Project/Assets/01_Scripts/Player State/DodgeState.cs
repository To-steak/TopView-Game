using UnityEngine;

namespace Player
{
    public class DodgeState : BaseState
    {
        public DodgeState(PlayerMediator mediator) : base(mediator) { }

        public override void Enter()
        {
            _mediator.Locomotions.SetRotationLock(true);

            Vector3 input = _mediator.Inputs.MoveInput;
            if (input != Vector3.zero)
            {
                _mediator.Locomotions.SetDirection(input);
            }

            _mediator.Locomotions.DoDodge();
            _mediator.Health.UseMana(_mediator.Health.DodgeCost);
            _mediator.Animations.PlayDodge();
        }

        public override void Exit()
        {
            _mediator.Locomotions.SetRotationLock(false);
        }
    }
}