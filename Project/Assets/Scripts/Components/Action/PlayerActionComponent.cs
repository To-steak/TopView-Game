using System;

namespace TopView.Components
{
    // 플레이어 행동 상태의 길이. 무기 공격의 길이는 WeaponComponent에 있다
    [Serializable]
    public struct PlayerActionComponent
    {
        // 회피 상태의 길이(틱)
        public int dodgeTicks;
        // 착지 동작 상태의 길이(틱)
        public int landTicks;
        // 무기 교체 상태에 들어간 뒤 무기가 바뀌기까지의 시간(틱)
        public int swapStartTicks;
        // 무기 교체 상태의 길이(틱)
        public int swapTicks;
        // 재장전 상태의 길이(틱). 끝날 때 탄약이 채워진다
        public int reloadTicks;
        // 사망 상태에 들어간 뒤 게임오버를 요청하기까지의 시간(틱)
        public int dieTicks;
    }
}
