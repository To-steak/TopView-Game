using System;

namespace TopView.Components
{
    // 연달아 공격하는 간격. 이 컴포넌트가 없는 무기는 공격 상태가 끝나면 바로 다시 공격할 수 있다.
    [Serializable]
    public struct AttackIntervalComponent
    {
        // 공격 상태에 들어간 뒤 다음 공격 상태에 들어갈 수 있기까지의 시간(틱). 공격 상태가 끝나는 시간보다 짧으면 효과가 없다
        public int intervalTicks;
    }
}
