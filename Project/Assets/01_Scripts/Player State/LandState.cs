using UnityEngine;

namespace Player
{
    public class LandState : BaseState
    {
        public LandState(PlayerMediator mediator) : base(mediator) { }

        public override void Tick()
        {
            Vector3 input = _mediator.Inputs.MoveInput;
            _mediator.Locomotions.SetDirection(input);
            _mediator.Animations.SetMoveAnimation(input);
        }
    }
}