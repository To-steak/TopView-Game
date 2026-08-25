using UnityEngine;

namespace Player
{
    public class ReloadState : BaseState
    {
        public ReloadState(PlayerMediator mediator) : base(mediator) { }

        public override void Enter()
        {
            _mediator.Locomotions.SetDirection(Vector3.zero);
            _mediator.Animations.PlayReload();
        }

        public override void HandleAnimationFinished()
        {
            _mediator.Weapons.Reload();

            base.HandleAnimationFinished();
        }
    }
}