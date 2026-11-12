using UnityEngine;

namespace TopView.Components
{
    // 런타임 값. 플레이어의 이동 상태.
    public struct PlayerMotionComponent
    {
        // 이동 방향(월드, XZ). 크기는 입력 크기다
        public Vector3 moveDirection;
        // 지금 이동 속도(유닛/초). 상태가 바꾸기 전까지 유지된다
        public float currentSpeed;
        // 위아래 속도(유닛/초)
        public float verticalVelocity;
        // 이번 틱에 땅에 닿아 있는지
        public bool isGrounded;
        // 조준 방향으로 도는 것을 막는지
        public bool isRotationLocked;
        // 마지막으로 이동 입력을 반영할 때 달렸는지. View가 이동 애니메이션 속도에 쓴다
        public bool isRunning;
    }
}
