using System;

namespace TopView.Components
{
    // 공격 상태의 길이. 이 컴포넌트가 없는 무기는 마지막 발이 나가면 공격 상태가 바로 끝난다.
    [Serializable]
    public struct AttackDurationComponent
    {
        // 공격 상태에 들어간 뒤 공격 상태가 끝나기까지의 시간(틱). 공격이 모두 나가기 전에는 지나도 끝나지 않는다
        public int durationTicks;
    }
}
