using UnityEngine;

namespace Player
{
    public class JumpState : BaseState
    {
        public JumpState(PlayerMediator mediator) : base(mediator) { }

        public override void Enter()
        {
            _mediator.Locomotions.DoJump();
            _mediator.Health.UseMana(_mediator.Health.JumpCost);
            _mediator.Animations.SetJump(true);
            _mediator.Locomotions.SetSpeed(_mediator.Locomotions.InAirSpeed);
        }

        public override void Tick()
        {
            Vector3 input = _mediator.Inputs.MoveInput;
            _mediator.Locomotions.SetDirection(input);

            if (_mediator.Locomotions.VerticalVelocity <= 0f && _mediator.Locomotions.IsGround)
            {
                _mediator.Animations.SetJump(false);
                _mediator.ChangeState(_mediator.LandState);
            }
        }
    }
}