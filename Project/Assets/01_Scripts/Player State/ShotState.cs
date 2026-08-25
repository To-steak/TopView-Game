using UnityEngine;

namespace Player
{
    public class ShotState : BaseState
    {
        public ShotState(PlayerMediator mediator) : base(mediator) { }

        public override void Enter()
        {
            _mediator.Locomotions.SetRotationLock(true);
            _mediator.Locomotions.SetDirection(Vector3.zero);
            _mediator.Animations.PlayShot();
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