using TopView.Components;
using TopView.Entities;

namespace TopView.Records
{
    // 적의 행동 상태가 바뀌었다
    public struct EnemyStateChangedRecord
    {
        // 상태가 바뀐 적
        public EntityIdentifier enemy;
        // 바뀌기 전 상태
        public EnemyState previousState;
        // 바뀐 상태
        public EnemyState state;
        // state가 Attack일 때 쓰는 공격 종류
        public EnemyAttackKind attackKind;
    }
}
