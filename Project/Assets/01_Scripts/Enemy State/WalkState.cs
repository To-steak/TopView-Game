using UnityEngine;

namespace Enemy
{
    public class WalkState : BaseState
    {
        public WalkState(EnemyMediator mediator) : base(mediator) { }

        public override void Enter()
        {
            _mediator.Animations.SetWalk(true);
        }

        public override void Tick()
        {
            _target = _mediator.EnemyAI.DetectPlayer(); // Idle State 에서 넘어와도 감지한다.
            if (_target != null)
            {
                _mediator.EnemyAI.SetMoveToTarget(_target);
            }

            if (_mediator.EnemyAI.HasReachedDestination()) // 공격 범위에 닿았다면?
            {
                if (_target != null)
                {
                    _mediator.ChangeState(_mediator.AttackState);
                }
                else
                {
                    _mediator.ChangeState(_mediator.IdleState);
                }
            }
        }
    }
}