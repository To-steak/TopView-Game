using System;

namespace TopView.Components
{
    // 보스의 행동 값
    [Serializable]
    public struct BossBehaviorComponent
    {
        // 대기 중 플레이어 쪽으로 도는 빠르기. 틱마다 남은 각도의 (이 값 × 틱 길이)만큼 돈다
        public float rotationSpeed;
        // 대기 상태에 들어간 뒤 공격하기까지의 시간(틱)
        public int attackEntryTicks;
        // 도발 상태의 길이(틱)
        public int tauntTicks;
        // 사망 상태에 들어간 뒤 사라지고 게임오버를 요청하기까지의 시간(틱)
        public int dieTicks;
    }
}
