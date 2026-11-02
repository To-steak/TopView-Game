using System;

namespace TopView.Components
{
    // 적의 감지, 배회, 공격 행동 값
    [Serializable]
    public struct EnemyBehaviorComponent
    {
        // 플레이어를 감지하는 반지름(유닛)
        public float detectionRadius;
        // 플레이어를 감지하지 못했을 때 새 배회 지점을 고르기까지의 시간(틱)
        public int wanderIntervalTicks;
        // 플레이어를 쫓을 때 멈추는 거리(유닛). 여기에 닿으면 공격한다
        public float attackDistance;
        // 공격이 나가기 전까지 플레이어 쪽으로 도는 빠르기. 틱마다 남은 각도의 (이 값 × 틱 길이)만큼 돈다
        public float attackRotationSpeed;
        // 사망 상태에 들어간 뒤 사라지기까지의 시간(틱)
        public int dieTicks;
    }
}
