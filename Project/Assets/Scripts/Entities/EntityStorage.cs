using System.Collections.Generic;
using TopView.Components;

namespace TopView.Entities
{
    // 공유 저장소. 엔티티는 칸 번호이고, 컴포넌트마다 같은 번호로 읽는 배열을 둔다. 지금은 투사체와 근접 판정만 담는다.
    // 칸은 엔티티 종류별 구간으로 나눈다. 구간마다 칸 수가 정해져 있어 한 종류가 다른 종류의 칸을 차지하지 않는다.
    public class EntityStorage
    {
        // 칸이 가진 컴포넌트 표시. None이면 빈 칸이다
        public ComponentMask[] masks;
        // 칸이 채워진 횟수
        public int[] generations;

        // 투사체 구간의 빈 칸 번호 목록. ProjectileSpawnSystem이 끝에서부터 꺼낸다
        public List<int> freeProjectileIndices;
        // 근접 판정 구간의 빈 칸 번호 목록. MeleeSpawnSystem이 끝에서부터 꺼낸다
        public List<int> freeMeleeIndices;

        // 위치
        public PositionComponent[] positions;
        // 회전
        public RotationComponent[] rotations;
        // 투사체 값. 생성 때 무기나 공격의 값에서 복사한다
        public ProjectileComponent[] projectiles;
        // 투사체의 이동 상태
        public ProjectileMotionComponent[] projectileMotions;
        // 공격의 진영과 피해량
        public AttackPayloadComponent[] attackPayloads;
        // 근접 판정 값과 진행 시간
        public MeleeComponent[] melees;
        // 근접 판정이 이미 맞힌 대상
        public MeleeHitsComponent[] meleeHits;

        /// <summary>
        /// 모든 칸이 빈 저장소를 만들고, 근접 판정 구간의 칸마다 이미 맞힌 대상을 기록할 배열을 할당한다.
        /// </summary>
        /// <param name="projectileCapacity">투사체 칸 수</param>
        /// <param name="meleeCapacity">근접 판정 칸 수</param>
        /// <param name="meleeHitCapacity">근접 판정 하나가 기록할 수 있는 이미 맞힌 대상 수</param>
        public EntityStorage(int projectileCapacity, int meleeCapacity, int meleeHitCapacity)
        {
            int capacity = projectileCapacity + meleeCapacity;
            masks = new ComponentMask[capacity];
            generations = new int[capacity];
            positions = new PositionComponent[capacity];
            rotations = new RotationComponent[capacity];
            projectiles = new ProjectileComponent[capacity];
            projectileMotions = new ProjectileMotionComponent[capacity];
            attackPayloads = new AttackPayloadComponent[capacity];
            melees = new MeleeComponent[capacity];
            meleeHits = new MeleeHitsComponent[capacity];

            freeProjectileIndices = new List<int>(projectileCapacity);
            for (int index = projectileCapacity - 1; index >= 0; index--)
            {
                freeProjectileIndices.Add(index);
            }

            freeMeleeIndices = new List<int>(meleeCapacity);
            for (int index = capacity - 1; index >= projectileCapacity; index--)
            {
                meleeHits[index].targets = new EntityReference[meleeHitCapacity];
                freeMeleeIndices.Add(index);
            }
        }
    }
}
