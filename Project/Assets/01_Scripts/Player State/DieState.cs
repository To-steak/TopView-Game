using UnityEngine;

namespace Player
{
    public class DieState : BaseState
    {
        private bool _isAnimationStarted;

        public DieState(PlayerMediator mediator) : base(mediator) { }

        public override void Enter()
        {
            _isAnimationStarted = false;
            _mediator.Locomotions.SetDirection(Vector3.zero);
            _mediator.Locomotions.SetRotationLock(true);
            _mediator.Animations.PlayDie();
            _isAnimationStarted = true;
        }

        public override void HandleAnimationFinished()
        {
            if (!_isAnimationStarted)
            {
                return;
            }
            GameEvents.CallbackPlayerDied();
        }
    }
}