using System;

namespace TopView.Components
{
    // 적 공격 하나의 값
    [Serializable]
    public struct EnemyAttackComponent
    {
        // 공격 종류
        public EnemyAttackKind kind;
        // 이 공격을 시작할 수 있는 플레이어까지의 거리(유닛, XZ). 플레이어 반지름을 더해 비교한다
        public float attackRange;
        // 공격 상태가 끝난 뒤 이 공격을 다시 고를 수 있을 때까지의 시간(틱). 0이면 바로 고를 수 있다
        public int cooldownTicks;
        // 공격 상태에 들어간 뒤 Body 판정이 시작되기까지의 시간(틱). Body만 쓴다. 투사체 공격은 shots의 칸마다 정한다
        public int startTicks;
        // 공격 상태에 들어간 뒤 Body 판정이 끝나기까지의 시간(틱). startTicks보다 커야 한다. Body만 쓴다
        public int endTicks;
        // 공격 상태에 들어간 뒤 공격 상태가 끝나기까지의 시간(틱). 공격이 모두 나가기 전에는 끝나지 않는다. Body는 endTicks 이후가 판정 없는 후딜레이다
        public int durationTicks;
        // 공격력에 곱하는 배율
        public float damageMultiplier;
        // Body 판정 구의 반지름(유닛). Body만 쓴다
        public float bodyRadius;
        // Body 판정 구 중심을 적 앞쪽으로 띄우는 거리(유닛). Body만 쓴다
        public float bodyOffset;
        // 내보내는 투사체. Body가 아닌 공격이 쓴다
        public ProjectileComponent projectile;
        // 투사체를 내보내는 발마다의 시점과 위치. startTicks 오름차순으로 둔다. Body가 아닌 공격이 쓴다
        public ProjectileShotComponent[] shots;
    }
}
