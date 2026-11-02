using System;
using UnityEngine;

namespace TopView.Components
{
    // 플레이어의 이동 값
    [Serializable]
    public struct PlayerMovementComponent
    {
        // 걷기 속도(유닛/초)
        public float walkSpeed;
        // 달리기 속도(유닛/초)
        public float runSpeed;
        // 점프할 때의 위쪽 속도(유닛/초)
        public float jumpPower;
        // 회피 속도(유닛/초)
        public float dodgeSpeed;
        // 점프 중과 착지 동작 중의 이동 속도(유닛/초)
        public float inAirSpeed;
        // 중력 가속도(유닛/초²). 아래쪽이 음수
        public float gravity;
        // 땅에 닿아 있을 때 아래로 누르는 속도(유닛/초). 음수
        public float groundPressure;
        // 접지 판정 구의 중심. 플레이어 위치 기준
        public Vector3 groundCheckOffset;
        // 접지 판정 구의 반지름(유닛)
        public float groundCheckRadius;
        // 접지 판정 레이어
        public LayerMask groundLayer;
        // 이동 충돌 캡슐의 반지름(유닛)
        public float capsuleRadius;
        // 이동 충돌 캡슐의 높이(유닛)
        public float capsuleHeight;
        // 이동 충돌 캡슐의 중심. 플레이어 회전 기준의 로컬 위치
        public Vector3 capsuleCenter;
        // 이동 충돌 시 장애물과 띄워 둘 거리(유닛)
        public float skinWidth;
        // 이동을 막는 레이어
        public LayerMask collisionLayer;
        // 투사체가 생기는 위치. 플레이어 회전 기준의 로컬 위치
        public Vector3 muzzleOffset;
    }
}
