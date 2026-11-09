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
        // 지금 공격 상태에서 쓰는 공격 번호
        public int attackIndex;
        // 공격별로 다시 고를 수 있을 때까지 남은 시간(틱). 재사용하는 배열이며 번호는 attacks와 같다
        public int[] attackCooldownRemainingTicks;
        // 지금 공격 상태에서 이미 내보낸 발 수. Body는 판정이 나가면 1이다. 다음에 내보낼 shots 칸 번호이기도 하다
        public int committedShotCount;
    }
}
