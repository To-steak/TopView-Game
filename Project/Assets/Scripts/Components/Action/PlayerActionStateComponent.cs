using TopView.Entities;

namespace TopView.Components
{
    // 런타임 값. 플레이어의 행동 상태와 공격 진행.
    public struct PlayerActionStateComponent
    {
        // 행동 상태
        public PlayerState state;
        // 지금 상태에 들어간 뒤 지난 시간(틱). 들어간 틱은 0이다
        public int elapsedTicks;
        // 지금 공격 상태에서 공격이 이미 나갔는지
        public bool isAttackCommitted;
        // 다음 공격 상태에 들어갈 수 있기까지 남은 시간(틱). 0이면 들어갈 수 있다
        public int attackCooldownTicks;
        // 투척 충전량(0~100). ThrowCharge 상태에서 매 틱 정하고, Throw 상태에서 던질 거리를 정할 때 쓴다
        public int throwCharge;
    }
}
