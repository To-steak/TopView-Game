namespace Boss
{
    public abstract class BaseState
    {
        protected BossMediator _mediator;

        public BaseState(BossMediator mediator)
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