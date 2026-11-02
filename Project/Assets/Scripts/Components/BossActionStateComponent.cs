using TopView.Entities;

namespace TopView.Components
{
    // 런타임 값. 보스의 행동 상태와 공격 진행.
    public struct BossActionStateComponent
    {
        // 행동 상태
        public BossState state;
        // 지금 상태에 들어간 뒤 지난 시간(틱). 들어간 틱은 0이다
        public int elapsedTicks;
        // 지금 공격 상태에서 쓰는 공격 번호
        public int attackIndex;
        // 지금 공격 상태에서 공격이 이미 나갔는지
        public bool isAttackCommitted;
        // BossMissile을 쏜 횟수. 짝수·홀수로 발사 위치를 번갈아 고른다
        public int missileShotCount;
    }
}
