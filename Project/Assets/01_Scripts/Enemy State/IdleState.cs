using UnityEngine;

namespace Enemy
{
    public class IdleState : BaseState
    {
        private float _timer;

        public IdleState(EnemyMediator mediator) : base(mediator) { }

        public override void Enter()
        {
            _mediator.Animations.PlayIdle();
            _timer = 0;
        }

        public override void Tick()
        {
            _target = _mediator.EnemyAI.DetectPlayer(); // 감지

            if (_target != null) // 감지 했다면?
            {
                _mediator.EnemyAI.SetMoveToTarget(_target); // 추적
                _mediator.ChangeState(_mediator.WalkState);
                return; 
            }

            // 감지 못 했다면?
            _timer += Time.deltaTime;
            if (_timer >= _mediator.EnemyAI.NextStepTime)
            {
                _timer = 0;

                if (_mediator.EnemyAI.GetRandomPosition(out Vector3 position)) // 무작위 횡보
                {
                    _mediator.EnemyAI.SetMoveToPosition(position);
                    _mediator.ChangeState(_mediator.WalkState);
                }
            }
        }
    }
}