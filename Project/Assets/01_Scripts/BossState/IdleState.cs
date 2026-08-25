using UnityEngine;

namespace Boss
{
    public class IdleState : BaseState
    {
        private float _timer;

        public IdleState(BossMediator mediator) : base(mediator) { }

        public override void Enter()
        {
            _mediator.Animations.PlayIdle();
            _timer = 0f;
        }

        public override void Tick()
        {
            _timer += Time.deltaTime;
            if (_timer >= _mediator.AttackEntryTime)
            {
                _mediator.ChangeState(_mediator.AttackState);
                return;
            }

            if (_mediator.Player == null) return;

            Vector3 direction = _mediator.Player.position - _mediator.transform.position;
            direction.y = 0f;

            if (direction.sqrMagnitude > 0.001f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);

                _mediator.transform.rotation = Quaternion.Slerp(
                    _mediator.transform.rotation,
                    targetRotation,
                    Time.deltaTime * _mediator.RotationSpeed
                );
            }
        }
    }
}