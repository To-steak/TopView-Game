using UnityEngine;

namespace Enemy
{
    public class DieState : BaseState
    {
        public DieState(EnemyMediator mediator) : base(mediator) { }

        public override void Enter()
        {
            _mediator.Animations.PlayDie();
            _mediator.EnemyAI.StopMove();
            
            GameEvents.CallbackEntityDied(_mediator.Health.Score * _mediator.Health.Level);
        }

        public override void HandleAnimationFinished()
        {
            _mediator.ReturnPool();
        }
    }
}