using UnityEngine;

namespace Player
{
    public class ThrowState : BaseState
    {
        public ThrowState(PlayerMediator mediator) : base(mediator) { }

        public override void Enter()
        {
            _mediator.Locomotions.SetDirection(Vector3.zero);
            _mediator.Locomotions.SetRotationLock(true);
            _mediator.Animations.PlayThrow();
        }

        public override void Tick()
        {
            _mediator.Weapons.TickAttack();
        }
        
        public override void HandleAnimationCommit()
        {
            _mediator.Weapons.BeginAttack();
        }

        public override void Exit()
        {
            _mediator.Locomotions.SetRotationLock(false);
            _mediator.Weapons.EndAttack();
        }
    }
}