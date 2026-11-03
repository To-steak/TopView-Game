using TopView.Entities;
using UnityEngine;

namespace TopView.Requests
{
    // 플레이어의 행동 상태를 바꾸라는 요청. 플레이어 System들이 넣고, 바로 뒤의 PlayerStateChangeSystem이 처리한다
    public struct PlayerStateChangeRequest
    {
        // 상태를 바꿀 플레이어
        public EntityIdentifier target;
        // 새 상태
        public PlayerState state;
        // 이동 입력. 회피 방향에 쓴다
        public Vector3 moveInput;
    }
}
