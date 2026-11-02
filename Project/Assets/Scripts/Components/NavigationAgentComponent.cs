using UnityEngine;

namespace TopView.Components
{
    // 런타임 값. 길찾기로 움직이는 엔티티의 목적지와 경로.
    public struct NavigationAgentComponent
    {
        // 최고 이동 속도(유닛/초). 생성 때 레벨로 계산한다
        public float maxSpeed;
        // 목적지가 있는지
        public bool hasDestination;
        // 목적지(월드)
        public Vector3 destination;
        // 목적지에서 이 거리 안에 들면 멈춘다(유닛)
        public float stoppingDistance;
        // 목적지가 바뀌어 경로를 다시 구해야 하는지
        public bool isPathDirty;
        // 지난 이동 틱에 목적지의 멈추는 거리 안에 들었는지. 목적지를 새로 정하면 false가 된다
        public bool hasReachedDestination;
        // 경로의 꺾이는 점. 재사용하는 배열이며 pathCornerCount까지만 쓴다
        public Vector3[] pathCorners;
        // 경로의 꺾이는 점 수
        public int pathCornerCount;
        // 다음에 향할 꺾이는 점 번호
        public int pathCornerIndex;
        // 이번 틱의 속도(월드, XZ)
        public Vector3 velocity;
    }
}
