using System.Collections.Generic;
using TopView.Components;

namespace TopView.Entities
{
    // 공유 저장소. 엔티티는 칸 번호이고, 컴포넌트마다 같은 번호로 읽는 배열을 둔다. 모든 엔티티를 담는다.
    // 칸은 엔티티 종류별 구간으로 나눈다. 구간마다 칸 수가 정해져 있어 한 종류가 다른 종류의 칸을 차지하지 않는다.
    public class EntityStorage
    {
        // 칸이 가진 컴포넌트 표시. None이면 빈 칸이다
        public ComponentMask[] masks;
        // 칸이 채워진 횟수
        public int[] generations;

        // 플레이어 구간의 빈 칸 번호 목록. PlayerSpawnSystem이 끝에서부터 꺼낸다
        public List<int> freePlayerIndices;

        // 위치
        public PositionComponent[] positions;
        // 회전
        public RotationComponent[] rotations;
        // 체력
        public HealthComponent[] healths;
        // 마나
        public ManaComponent[] manas;
        // 플레이어 이동 값. 생성 때 Definition에서 복사한다
        public PlayerMovementComponent[] playerMovements;
        // 플레이어 체력과 마나 값. 생성 때 Definition에서 복사한다
        public PlayerVitalsComponent[] playerVitals;
        // 플레이어 행동 상태의 길이. 생성 때 Definition에서 복사한다
        public PlayerActionComponent[] playerActions;
        // 플레이어 행동 상태와 공격 진행
        public PlayerActionStateComponent[] playerActionStates;
        // 플레이어 이동 상태
        public PlayerMotionComponent[] playerMotions;
        // 무기 목록
        public WeaponListComponent[] weaponLists;
        // 든 무기와 무기별 탄약
        public PlayerWeaponSlotsComponent[] weaponSlots;

        /// <summary>
        /// 모든 칸이 빈 저장소를 만든다.
        /// </summary>
        /// <param name="capacity">플레이어 칸 수</param>
        public EntityStorage(int capacity)
        {
            masks = new ComponentMask[capacity];
            generations = new int[capacity];
            positions = new PositionComponent[capacity];
            rotations = new RotationComponent[capacity];
            healths = new HealthComponent[capacity];
            manas = new ManaComponent[capacity];
            playerMovements = new PlayerMovementComponent[capacity];
            playerVitals = new PlayerVitalsComponent[capacity];
            playerActions = new PlayerActionComponent[capacity];
            playerActionStates = new PlayerActionStateComponent[capacity];
            playerMotions = new PlayerMotionComponent[capacity];
            weaponLists = new WeaponListComponent[capacity];
            weaponSlots = new PlayerWeaponSlotsComponent[capacity];

            freePlayerIndices = new List<int>(capacity);
            for (int index = capacity - 1; index >= 0; index--)
            {
                freePlayerIndices.Add(index);
            }
        }
    }
}
