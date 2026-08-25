using UnityEngine;

namespace Enemy
{
    public abstract class BaseState
    {
        protected EnemyMediator _mediator;
        protected Transform _target;

        public BaseState(EnemyMediator mediator)
        {
            _mediator = mediator;
        }

        public virtual void Enter() { }
        public virtual void Tick() { }
        public virtual void Exit() { }
        
        public virtual void HandleAttackCommit() { }
        public virtual void HandleAnimationFinished() { }
    }
}