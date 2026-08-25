using UnityEngine;

namespace Boss
{
    public class DieState : BaseState
    {
        public DieState(BossMediator mediator) : base(mediator) { }

        public override void Enter()
        {
            _mediator.Animations.PlayDie();
            GameEvents.CallbackEntityDied(_mediator.Health.Score * _mediator.Health.Level);
        }

        public override void HandleAnimationFinished()
        {
            _mediator.gameObject.SetActive(false);
            GameEvents.CallbackBossDied();
        }
    }
}