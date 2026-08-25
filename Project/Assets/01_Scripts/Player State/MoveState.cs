using UnityEngine;

namespace Player
{
    public class MoveState : BaseState
    {
        public MoveState(PlayerMediator mediator) : base(mediator) { }

        public override void Tick()
        {
            Vector3 input = _mediator.Inputs.MoveInput;
            bool isRunning = _mediator.Inputs.IsRunning;

            if (input == Vector3.zero)
            {
                _mediator.ChangeState(_mediator.IdleState);
                return;
            }

            if (isRunning)
            {
                float requiredMana = _mediator.Health.RunCost * Time.deltaTime;
                if (_mediator.Health.HasEnoughMana(requiredMana))
                {
                    _mediator.Health.UseMana(requiredMana);
                }
                else
                {
                    isRunning = false;
                }
            }

            _mediator.Locomotions.SetDirection(input);

            float speed = isRunning ? _mediator.Locomotions.RunSpeed : _mediator.Locomotions.WalkSpeed;
            _mediator.Locomotions.SetSpeed(speed);

            _mediator.Animations.SetMoveAnimation(input);
            _mediator.Animations.SetRunAnimation(isRunning);
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

        public override void HandleDodge()
        {
            if (_mediator.Locomotions.IsGround)
            {
                if (_mediator.Health.HasEnoughMana(_mediator.Health.DodgeCost))
                {
                    _mediator.ChangeState(_mediator.DodgeState);
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
    }
}