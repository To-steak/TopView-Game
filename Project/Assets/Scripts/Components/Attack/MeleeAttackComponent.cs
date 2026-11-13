using System;

namespace TopView.Components
{
    // 근접 판정을 내보내는 공격의 값. 근접 무기와 적의 Body 공격이 쓴다. 시간은 모두 공격 상태에 들어간 때부터 잰다.
    [Serializable]
    public struct MeleeAttackComponent
    {
        // 공격 상태에 들어간 뒤 판정이 시작되기까지의 시간(틱)
        public int startTicks;
        // 공격 상태에 들어간 뒤 판정이 끝나기까지의 시간(틱). startTicks보다 커야 한다
        public int endTicks;
        // 판정 구의 크기와 위치
        public MeleeShapeComponent shape;
    }
}
