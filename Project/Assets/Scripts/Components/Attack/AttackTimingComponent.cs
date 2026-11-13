using System;

namespace TopView.Components
{
    // 공격 상태 한 번의 시간 구간. 모두 공격 상태에 들어간 때부터 잰다. 공격 종류마다 쓰는 칸은 이 값을 가진 쪽에 적는다.
    [Serializable]
    public struct AttackTimingComponent
    {
        // 공격 상태에 들어간 뒤 공격이 나가기까지의 시간(틱). 근접은 판정이 시작된다
        public int startTicks;
        // 공격 상태에 들어간 뒤 근접 판정이 끝나기까지의 시간(틱). startTicks보다 커야 한다
        public int endTicks;
        // 공격 상태에 들어간 뒤 공격 상태가 끝나기까지의 시간(틱). 근접은 endTicks 이후가 판정 없는 후딜레이다
        public int durationTicks;
    }
}
