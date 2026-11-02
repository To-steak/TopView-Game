using TopView.Components;

namespace TopView.Entities
{
    // 플레이어 하나의 데이터
    public struct PlayerEntity
    {
        // 살아 있는 칸인지. false면 빈 칸이다
        public bool isAlive;
        // 이 칸이 채워진 횟수
        public int generation;

        // 이동 값. 생성 때 Definition에서 복사한다
        public PlayerMovementComponent movement;
        // 체력과 마나 값. 생성 때 Definition에서 복사한다
        public PlayerVitalsComponent vitals;
        // 행동 상태의 길이. 생성 때 Definition에서 복사한다
        public PlayerActionComponent action;

        // 위치
        public PositionComponent position;
        // 회전
        public RotationComponent rotation;

        // 행동 상태와 공격 진행
        public PlayerActionStateComponent actionState;
        // 이동 상태
        public PlayerMotionComponent motion;

        // 체력
        public HealthComponent health;
        // 마나
        public ManaComponent mana;

        // 무기 목록
        public WeaponListComponent weaponList;
        // 든 무기와 무기별 탄약
        public PlayerWeaponSlotsComponent weaponSlots;
    }
}
