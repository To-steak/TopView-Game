using TopView.Components;
using TopView.Entities;

namespace TopView.Records
{
    // 보스의 행동 상태가 바뀌었다
    public struct BossStateChangedRecord
    {
        // 상태가 바뀐 보스
        public EntityIdentifier boss;
        // 바뀌기 전 상태
        public BossState previousState;
        // 바뀐 상태
        public BossState state;
        // state가 Attack일 때 쓰는 공격 종류
        public EnemyAttackKind attackKind;
    }
}
