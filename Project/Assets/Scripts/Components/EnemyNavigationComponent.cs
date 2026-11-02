using System;

namespace TopView.Components
{
    // 적의 길찾기 이동 값
    [Serializable]
    public struct EnemyNavigationComponent
    {
        // 가속도(유닛/초²). 감속과 도착 감속에도 쓴다
        public float acceleration;
        // 걸을 때 진행 방향으로, 공격이 나가기 전에 플레이어 쪽으로 도는 최고 각속도(도/초)
        public float angularSpeed;
    }
}
