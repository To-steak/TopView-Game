using TopView.Components;
using UnityEngine;

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

        // 행동 상태
        public PlayerState state;
        // 지금 상태에 들어간 뒤 지난 시간(틱). 들어간 틱은 0이다
        public int stateElapsedTicks;
        // 지금 공격 상태에서 공격이 이미 나갔는지
        public bool isAttackCommitted;
        // 다음 공격 상태에 들어갈 수 있기까지 남은 시간(틱). 0이면 들어갈 수 있다
        public int attackCooldownTicks;

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

        // 체력
        public HealthComponent health;
        // 마나
        public float mana;
        // 마지막으로 마나를 쓴 뒤 지난 시간(틱). 회복 지연 시간에 닿으면 더 늘리지 않는다
        public int manaRecoveryElapsedTicks;

        // 무기 목록. 생성 때 Definition에서 복사한다
        public WeaponComponent[] weapons;
        // 무기별 장전된 탄약 수. 재사용하는 배열이며 번호는 weapons와 같다
        public int[] weaponCurrentAmmo;
        // 무기별 예비 탄약 수. 재사용하는 배열이며 번호는 weapons와 같다
        public int[] weaponReserveAmmo;
        // 들고 있는 무기 번호
        public int currentWeaponIndex;
        // 교체 상태에서 swapStartTicks에 바꿀 무기 번호
        public int pendingWeaponIndex;
    }
}
