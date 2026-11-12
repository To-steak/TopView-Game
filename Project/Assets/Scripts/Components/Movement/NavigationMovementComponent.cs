using System;

namespace TopView.Components
{
    // 길찾기로 움직이는 엔티티의 이동, 회전 값. 목적지와 경로는 NavigationAgentComponent에 있다
    [Serializable]
    public struct NavigationMovementComponent
    {
        // 플레이어를 쫓을 때 남은 경로가 이 거리 안이면 멈춘다(유닛)
        public float stopDistance;
        // 가속도(유닛/초²). 감속과 도착 감속에도 쓴다
        public float acceleration;
        // 걸을 때 진행 방향으로, 공격이 나가기 전에 플레이어 쪽으로 도는 최고 각속도(도/초)
        public float angularSpeed;
    }
}
