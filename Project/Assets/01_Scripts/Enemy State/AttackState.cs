using UnityEngine;

namespace Enemy
{
    public class AttackState : BaseState
    {
        private bool _isCommitted;
        private int _index;

        public AttackState(EnemyMediator mediator) : base(mediator) { }

        public override void Enter()
        {
            _mediator.Animations.SetWalk(false);
            _mediator.EnemyAI.StopMove();

            _target = _mediator.EnemyAI.DetectPlayer();
            _isCommitted = false;

            _index = Random.Range(0, _mediator.Attack.Count);
            var hash = _mediator.Attack.GetAnimationHash(_index);

            _mediator.Animations.PlayAttack(hash);
        }

        public override void Tick()
        {
            if (_target == null || _isCommitted) return;

            Vector3 direction = _target.position - _mediator.transform.position;
            direction.y = 0f;

            if (direction.sqrMagnitude > 0.001f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);

                _mediator.transform.rotation = Quaternion.Slerp(
                    _mediator.transform.rotation,
                    targetRotation,
                    Time.deltaTime * _mediator.EnemyAI.AttackRotationSpeed
                );
            }
        }

        public override void HandleAttackCommit()
        {
            _isCommitted = true;
            _mediator.Attack.Attack(_mediator.Health.AttackDamage, _index);
        }

        public override void HandleAnimationFinished()
        {
            _mediator.ChangeState(_mediator.IdleState);
        }
    }
}