using UnityEngine;

namespace Player
{
    public abstract class BaseState
    {
        protected PlayerMediator _mediator;

        public BaseState(PlayerMediator mediator)
        {
            _mediator = mediator;
        }

        public virtual void Enter() { }
        public virtual void Tick() { }
        public virtual void Exit() { }
        public virtual void HandleJump() { }
        public virtual void HandleDodge() { }
        public virtual void HandleAttack() { }
        public virtual void HandleWeaponSwap(int index) { }
        public virtual void HandleReload() { }

        public virtual void HandleAnimationCommit() { }
        public virtual void HandleAnimationFinished()
        {
            if (_mediator.Inputs.MoveInput == Vector3.zero)
            {
                _mediator.ChangeState(_mediator.IdleState);
            }
            else
            {
                _mediator.ChangeState(_mediator.MoveState);
            }
        }
    }
}
