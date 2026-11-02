using System.Collections.Generic;
using TopView.Components;
using UnityEngine;

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

        // 투사체 구간의 빈 칸 번호 목록. ProjectileSpawnSystem이 끝에서부터 꺼낸다
        public List<int> freeProjectileIndices;
        // 근접 판정 구간의 빈 칸 번호 목록. MeleeSpawnSystem이 끝에서부터 꺼낸다
        public List<int> freeMeleeIndices;
        // 적 종류별 구간의 빈 칸 번호 목록. EnemySpawnSystem이 끝에서부터 꺼낸다
        public List<int>[] freeEnemyIndicesByKind;
        // 보스 구간의 빈 칸 번호 목록. BossSpawnSystem이 끝에서부터 꺼낸다
        public List<int> freeBossIndices;
        // 플레이어 구간의 빈 칸 번호 목록. PlayerSpawnSystem이 끝에서부터 꺼낸다
        public List<int> freePlayerIndices;

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
        // 체력
        public HealthComponent[] healths;
        // 레벨과 공격력
        public CombatComponent[] combats;
        // 체력·공격력·점수 값. 생성 때 Definition에서 복사한다
        public EnemyStatsComponent[] enemyStats;
        // 공격 목록
        public AttackListComponent[] attackLists;
        // 보스 행동 값. 생성 때 Definition에서 복사한다
        public BossBehaviorComponent[] bossBehaviors;
        // 보스 행동 상태와 공격 진행
        public BossActionStateComponent[] bossActionStates;
        // 적 종류 번호
        public EnemyKindComponent[] enemyKinds;
        // 적 행동 값. 생성 때 Definition에서 복사한다
        public EnemyBehaviorComponent[] enemyBehaviors;
        // 적 행동 상태와 공격 진행
        public EnemyActionStateComponent[] enemyActionStates;
        // 목적지와 경로
        public NavigationAgentComponent[] navigationAgents;
        // 공격을 맞는 진영
        public FactionComponent[] factions;
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
        /// 모든 칸이 빈 저장소를 만들고, 근접 판정 구간의 칸마다 이미 맞힌 대상을 기록할 배열을, 적 구간의 칸마다 경로 배열을 할당한다.
        /// </summary>
        /// <param name="projectileCapacity">투사체 칸 수</param>
        /// <param name="meleeCapacity">근접 판정 칸 수</param>
        /// <param name="meleeHitCapacity">근접 판정 하나가 기록할 수 있는 이미 맞힌 대상 수</param>
        /// <param name="enemyKindCount">적 종류 수</param>
        /// <param name="enemyCapacityPerKind">적 종류별 칸 수</param>
        /// <param name="pathCornerCapacity">적 칸마다 미리 만들어 둘 경로 배열의 크기</param>
        /// <param name="bossCapacity">보스 칸 수</param>
        /// <param name="playerCapacity">플레이어 칸 수</param>
        public EntityStorage(int projectileCapacity, int meleeCapacity, int meleeHitCapacity, int enemyKindCount, int enemyCapacityPerKind, int pathCornerCapacity, int bossCapacity, int playerCapacity)
        {
            int meleeEnd = projectileCapacity + meleeCapacity;
            int enemyEnd = meleeEnd + enemyKindCount * enemyCapacityPerKind;
            int bossEnd = enemyEnd + bossCapacity;
            int capacity = bossEnd + playerCapacity;
            masks = new ComponentMask[capacity];
            generations = new int[capacity];
            positions = new PositionComponent[capacity];
            rotations = new RotationComponent[capacity];
            projectiles = new ProjectileComponent[capacity];
            projectileMotions = new ProjectileMotionComponent[capacity];
            attackPayloads = new AttackPayloadComponent[capacity];
            melees = new MeleeComponent[capacity];
            meleeHits = new MeleeHitsComponent[capacity];
            healths = new HealthComponent[capacity];
            combats = new CombatComponent[capacity];
            enemyStats = new EnemyStatsComponent[capacity];
            attackLists = new AttackListComponent[capacity];
            bossBehaviors = new BossBehaviorComponent[capacity];
            bossActionStates = new BossActionStateComponent[capacity];
            enemyKinds = new EnemyKindComponent[capacity];
            enemyBehaviors = new EnemyBehaviorComponent[capacity];
            enemyActionStates = new EnemyActionStateComponent[capacity];
            navigationAgents = new NavigationAgentComponent[capacity];
            factions = new FactionComponent[capacity];
            manas = new ManaComponent[capacity];
            playerMovements = new PlayerMovementComponent[capacity];
            playerVitals = new PlayerVitalsComponent[capacity];
            playerActions = new PlayerActionComponent[capacity];
            playerActionStates = new PlayerActionStateComponent[capacity];
            playerMotions = new PlayerMotionComponent[capacity];
            weaponLists = new WeaponListComponent[capacity];
            weaponSlots = new PlayerWeaponSlotsComponent[capacity];

            freeProjectileIndices = new List<int>(projectileCapacity);
            for (int index = projectileCapacity - 1; index >= 0; index--)
            {
                freeProjectileIndices.Add(index);
            }

            freeMeleeIndices = new List<int>(meleeCapacity);
            for (int index = meleeEnd - 1; index >= projectileCapacity; index--)
            {
                meleeHits[index].targets = new EntityIdentifier[meleeHitCapacity];
                freeMeleeIndices.Add(index);
            }

            freeEnemyIndicesByKind = new List<int>[enemyKindCount];
            for (int kind = 0; kind < enemyKindCount; kind++)
            {
                freeEnemyIndicesByKind[kind] = new List<int>(enemyCapacityPerKind);
                int kindStart = meleeEnd + kind * enemyCapacityPerKind;
                for (int index = kindStart + enemyCapacityPerKind - 1; index >= kindStart; index--)
                {
                    navigationAgents[index].pathCorners = new Vector3[pathCornerCapacity];
                    freeEnemyIndicesByKind[kind].Add(index);
                }
            }

            freeBossIndices = new List<int>(bossCapacity);
            for (int index = bossEnd - 1; index >= enemyEnd; index--)
            {
                freeBossIndices.Add(index);
            }

            freePlayerIndices = new List<int>(playerCapacity);
            for (int index = capacity - 1; index >= bossEnd; index--)
            {
                freePlayerIndices.Add(index);
            }
        }
    }
}
