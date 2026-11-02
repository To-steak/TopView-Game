using TopView.Entities;

namespace TopView.Components
{
    // 런타임 값. 적의 행동 상태와 공격 진행.
    public struct EnemyActionStateComponent
    {
        // 행동 상태
        public EnemyState state;
        // 지금 상태에 들어간 뒤 지난 시간(틱). 들어간 틱은 0이다
        public int elapsedTicks;
        // Idle 상태에서 마지막으로 배회 지점을 고른 뒤 지난 시간(틱)
        public int wanderElapsedTicks;
        // 지금 공격 상태에서 쓰는 공격 번호
        public int attackIndex;
        // 공격별로 다시 고를 수 있을 때까지 남은 시간(틱). 재사용하는 배열이며 번호는 attacks와 같다
        public int[] attackCooldownRemainingTicks;
        // 지금 공격 상태에서 공격이 이미 나갔는지
        public bool isAttackCommitted;
        // BossMissile을 쏜 횟수. 짝수·홀수로 발사 위치를 번갈아 고른다
        public int missileShotCount;
    }
}
