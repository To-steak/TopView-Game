using System;

namespace TopView.Components
{
    // 적의 길찾기 이동 값
    [Serializable]
    public struct EnemyNavigationComponent
    {
        // 최고 속도(유닛/초)
        public float speed;
        // 가속도(유닛/초²). 감속과 도착 감속에도 쓴다
        public float acceleration;
        // 진행 방향으로 도는 최고 각속도(도/초)
        public float angularSpeed;
        // 다른 적과 겹치지 않게 지키는 반지름(유닛)
        public float radius;
        // 화면에서 NavMesh 표면 위로 띄우는 높이(유닛)
        public float baseOffset;
        // 목적지와 생성 위치를 NavMesh 위로 맞출 때 찾는 최대 거리(유닛)
        public float navMeshSampleDistance;
        // 경로의 꺾이는 점을 담을 수 있는 최대 수
        public int maxPathCorners;
    }
}
