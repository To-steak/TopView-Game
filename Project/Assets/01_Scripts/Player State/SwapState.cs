using UnityEngine;

namespace Player
{
    public class SwapState : BaseState
    {
        public SwapState(PlayerMediator mediator) : base(mediator) { }

        public override void Enter()
        {
            _mediator.Locomotions.SetDirection(Vector3.zero);
            _mediator.Animations.PlaySwap();
            _mediator.Weapons.ExecuteSwap();
        }
    }
}