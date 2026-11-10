using TopView.Components;
using TopView.Entities;

namespace TopView.Records
{
    // 적이 공격을 내보냈다. Body는 판정이 시작될 때, 투사체 공격은 한 발이 나갈 때마다 남는다
    public struct EnemyAttackCommittedRecord
    {
        // 공격한 적
        public EntityIdentifier enemy;
        // 내보낸 공격의 종류
        public EnemyAttackKind attackKind;
    }
}
