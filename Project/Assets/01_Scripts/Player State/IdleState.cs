using UnityEngine;

namespace Player
{
    public class IdleState : BaseState
    {
        public IdleState(PlayerMediator mediator) : base(mediator) { }

        public override void Enter()
        {
            _mediator.Locomotions.SetDirection(Vector3.zero);
            _mediator.Animations.SetMoveAnimation(Vector3.zero);
        }

        public override void Tick()
        {
            Vector3 input = _mediator.Inputs.MoveInput;

            if (input != Vector3.zero)
            {
                _mediator.ChangeState(_mediator.MoveState);
            }
        }

        public override void HandleJump()
        {
            if (_mediator.Locomotions.IsGround)
            {
                if (_mediator.Health.HasEnoughMana(_mediator.Health.JumpCost))
                {
                    _mediator.ChangeState(_mediator.JumpState);
                }
            }
        }

        public override void HandleAttack()
        {
            if (_mediator.Locomotions.IsGround)
            {
                var state = _mediator.Weapons.GetAttackState(_mediator);
                if (state != null)
                {
                    _mediator.ChangeState(state);
                }
            }
        }

        public override void HandleWeaponSwap(int index)
        {
            if (_mediator.Weapons.CanSwap(index))
            {
                _mediator.Weapons.PendingSwap(index);
                _mediator.ChangeState(_mediator.SwapState);
            }
        }

        public override void HandleReload()
        {
            if (_mediator.Weapons.CanReload())
            {
                _mediator.ChangeState(_mediator.ReloadState);
            }
        }

        public override void HandleAnimationFinished()
        {
            // Do nothing
            // IDLE에는 SMB가 부착되어 있지 않아 호출되지 않음
        }
    }
}