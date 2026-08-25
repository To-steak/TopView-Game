using UnityEngine;

namespace Boss
{
    public class AttackState : BaseState
    {
        private int _index;

        public AttackState(BossMediator mediator) : base(mediator) { }

        public override void Enter()
        {
            _index = Random.Range(0, _mediator.Attack.Count);
            var hash = _mediator.Attack.GetAnimationHash(_index);
            _mediator.Animations.PlayAttack(hash);
        }

        public override void HandleAttackCommit()
        {
            _mediator.Attack.Attack(_mediator.Health.AttackDamage, _index);
        }

        public override void HandleAnimationFinished()
        {
            _mediator.ChangeState(_mediator.IdleState);
        }
    }
}