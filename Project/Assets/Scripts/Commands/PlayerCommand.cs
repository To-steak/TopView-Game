using UnityEngine;

namespace TopView.Commands
{
    // 플레이어 입력. Runner가 Update마다 갱신하고, 누름 입력은 틱이 끝날 때 CommandSystem이 지운다.
    public struct PlayerCommand
    {
        // 이동 방향(월드, XZ). 카메라 기준으로 바꾼 값이며 크기는 입력 크기다
        public Vector3 moveDirection;
        // 달리기 키를 누르고 있는지
        public bool isRunning;
        // 공격 키를 누르고 있는지
        public bool isAttackHeld;
        // aimPoint가 유효한지
        public bool hasAimPoint;
        // 마우스가 가리키는 총구 높이 평면 위의 점(월드)
        public Vector3 aimPoint;
        // 이번 틱 전에 점프 키를 눌렀는지
        public bool isJumpPressed;
        // 이번 틱 전에 회피 키를 눌렀는지
        public bool isDodgePressed;
        // 이번 틱 전에 공격 키를 눌렀는지
        public bool isAttackPressed;
        // 이번 틱 전에 재장전 키를 눌렀는지
        public bool isReloadPressed;
        // 이번 틱 전에 누른 무기 교체 번호. 음수면 없음
        public int swapWeaponIndex;
    }
}
