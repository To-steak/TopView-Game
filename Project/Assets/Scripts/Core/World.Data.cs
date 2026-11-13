using System.Collections.Generic;
using TopView.Commands;
using TopView.Components;
using TopView.Entities;
using TopView.Records;
using TopView.Requests;
using UnityEngine.AI;

namespace TopView.Core
{
    // 한 판의 데이터. World만 쓰고, System과 ViewSystem에는 필요한 것만 꺼내 넘긴다. 씬과 함께 생기고 사라져 새 판에 이전 값이 남지 않는다.
    public partial class World
    {
#if UNITY_EDITOR
        // 판이 준비되었는지. Editor의 WorldGizmos가 읽기만 한다. false면 아래 통로를 읽지 않는다
        public bool IsEditorRoundReady => _isRoundReady;
        // 플레이어 칸. Editor의 WorldGizmos가 읽기만 한다
        public PlayerEntity[] EditorPlayers => _players.entities;
        // 적 칸. Editor의 WorldGizmos가 읽기만 한다
        public EnemyEntity[] EditorEnemies => _enemies.entities;
        // 근접 판정 칸. Editor의 WorldGizmos가 읽기만 한다
        public MeleeEntity[] EditorMelees => _melees.entities;
        // 직선 투사체 칸. Editor의 WorldGizmos가 읽기만 한다
        public StraightProjectileEntity[] EditorStraightProjectiles => _straightProjectiles.entities;
        // 포물선 투사체 칸. Editor의 WorldGizmos가 읽기만 한다
        public ArcProjectileEntity[] EditorArcProjectiles => _arcProjectiles.entities;
        // 전리품 칸. Editor의 WorldGizmos가 읽기만 한다
        public LootEntity[] EditorLoots => _loots.entities;
        // 전리품 획득 반경. Editor의 WorldGizmos가 읽기만 한다
        public float EditorLootPickupRadius => _lootRule.pickupRadius;
#endif

        // 판이 준비되었는지. 판 준비를 마치면 true가 되고, 판을 비우면 false가 된다. false면 틱과 View가 돌지 않고, 판 준비 때 채우는 값은 비어 있다
        private bool _isRoundReady;
        // 게임 규칙 값. 판 시작 때 Definition에서 복사한다
        private GameRuleComponent _rule;
        // 전리품 규칙 값. 판 시작 때 Definition에서 복사한다
        private LootRuleComponent _lootRule;
        // 적 스포너 목록. 판 시작 때 켜진 Enemy Spawner Authoring에서 채운다
        private EnemySpawnerComponent[] _enemySpawners;
        // 적 스포너 상태 목록. 번호가 enemySpawners와 같다
        private EnemySpawnerStateComponent[] _enemySpawnerStates;
        // 플레이어 감지기 목록. 판 시작 때 켜진 Player Detector Authoring에서 채운다
        private PlayerDetectorComponent[] _playerDetectors;
        // 플레이어 감지기 상태 목록. 번호가 playerDetectors와 같다
        private PlayerDetectorStateComponent[] _playerDetectorStates;
        // 한 판의 진행 상태
        private GameProgressComponent _progress;
        // 이번 틱에 적이 노리는 플레이어의 위치. 매 틱 PlayerLocationSystem이 다시 채운다
        private PlayerLocationComponent _playerLocation;
        // 시드 고정 난수 생성기
        private System.Random _random;
        // 입력 명령 버퍼
        private readonly CommandBuffer _commandBuffer = new CommandBuffer();
        // 적 경로 계산에 다시 쓰는 NavMeshPath. MonoBehaviour의 필드 초기화에서는 만들 수 없어 판 시작 때 만든다
        private NavMeshPath _navigationPath;

        // 플레이어 저장소
        private PlayerStorage _players;
        // 적 저장소
        private EnemyStorage _enemies;
        // 무기 저장소
        private WeaponStorage _weapons;
        // 근접 판정 저장소
        private MeleeStorage _melees;
        // 직선 투사체 저장소
        private StraightProjectileStorage _straightProjectiles;
        // 포물선 투사체 저장소
        private ArcProjectileStorage _arcProjectiles;
        // 전리품 저장소
        private LootStorage _loots;
        // 이번 틱의 플레이어 피격 대상 목록. 적의 공격이 읽는다. 판 시작 때 플레이어 칸 수로 만들고, 매 틱 HitTargetSystem이 다시 채운다
        private HitTargetComponent[] _playerHitTargets;
        // 플레이어 피격 대상 목록에서 이번 틱에 채운 대상 수
        private int _playerHitTargetCount;
        // 이번 틱의 적 피격 대상 목록. 플레이어의 공격이 읽는다. 판 시작 때 적 칸 수로 만들고, 매 틱 HitTargetSystem이 다시 채운다
        private HitTargetComponent[] _enemyHitTargets;
        // 적 피격 대상 목록에서 이번 틱에 채운 대상 수
        private int _enemyHitTargetCount;

        // 판을 준비할 때 종류별로 Definition 값을 채워 둔 적 생성 요청. 번호가 적 종류 번호다
        private EnemySpawnRequest[] _enemySpawnTemplates;

        // 플레이어 생성 요청 큐
        private readonly Queue<PlayerSpawnRequest> _playerSpawnRequests = new Queue<PlayerSpawnRequest>();
        // 무기 생성 요청 큐
        private readonly Queue<WeaponSpawnRequest> _weaponSpawnRequests = new Queue<WeaponSpawnRequest>();
        // 적 생성 요청 큐
        private readonly Queue<EnemySpawnRequest> _enemySpawnRequests = new Queue<EnemySpawnRequest>();
        // 적 스포너 발동 요청 큐
        private readonly Queue<EnemySpawnerActivationRequest> _enemySpawnerActivationRequests = new Queue<EnemySpawnerActivationRequest>();
        // 직선 투사체 생성 요청 큐
        private readonly Queue<StraightProjectileSpawnRequest> _straightProjectileSpawnRequests = new Queue<StraightProjectileSpawnRequest>();
        // 포물선 투사체 생성 요청 큐
        private readonly Queue<ArcProjectileSpawnRequest> _arcProjectileSpawnRequests = new Queue<ArcProjectileSpawnRequest>();
        // 근접 판정 생성 요청 큐
        private readonly Queue<MeleeSpawnRequest> _meleeSpawnRequests = new Queue<MeleeSpawnRequest>();
        // 전리품 생성 요청 큐
        private readonly Queue<LootSpawnRequest> _lootSpawnRequests = new Queue<LootSpawnRequest>();
        // 플레이어 상태 변경 요청 큐
        private readonly Queue<PlayerStateChangeRequest> _playerStateChangeRequests = new Queue<PlayerStateChangeRequest>();
        // 적 상태 변경 요청 큐
        private readonly Queue<EnemyStateChangeRequest> _enemyStateChangeRequests = new Queue<EnemyStateChangeRequest>();
        // 직선 투사체 소멸 요청 큐
        private readonly Queue<ProjectileDespawnRequest> _straightProjectileDespawnRequests = new Queue<ProjectileDespawnRequest>();
        // 포물선 투사체 소멸 요청 큐
        private readonly Queue<ProjectileDespawnRequest> _arcProjectileDespawnRequests = new Queue<ProjectileDespawnRequest>();
        // 무기 소멸 요청 큐
        private readonly Queue<WeaponDespawnRequest> _weaponDespawnRequests = new Queue<WeaponDespawnRequest>();
        // 근접 판정 소멸 요청 큐
        private readonly Queue<MeleeDespawnRequest> _meleeDespawnRequests = new Queue<MeleeDespawnRequest>();
        // 전리품 소멸 요청 큐
        private readonly Queue<LootDespawnRequest> _lootDespawnRequests = new Queue<LootDespawnRequest>();
        // 플레이어 피해 요청 큐. 적의 공격이 넣는다
        private readonly Queue<DamageRequest> _playerDamageRequests = new Queue<DamageRequest>();
        // 적 피해 요청 큐. 플레이어의 공격이 넣는다
        private readonly Queue<DamageRequest> _enemyDamageRequests = new Queue<DamageRequest>();
        // 게임오버 요청 큐
        private readonly Queue<GameOverRequest> _gameOverRequests = new Queue<GameOverRequest>();

        // 이번 프레임의 플레이어 상태 변경 기록. LateUpdate 마지막에 비운다
        private readonly List<PlayerStateChangedRecord> _playerStateChangedRecords = new List<PlayerStateChangedRecord>();
        // 이번 프레임에 플레이어가 공격을 내보낸 기록. LateUpdate 마지막에 비운다
        private readonly List<PlayerAttackCommittedRecord> _playerAttackCommittedRecords = new List<PlayerAttackCommittedRecord>();
        // 이번 프레임의 적 상태 변경 기록. LateUpdate 마지막에 비운다
        private readonly List<EnemyStateChangedRecord> _enemyStateChangedRecords = new List<EnemyStateChangedRecord>();
        // 이번 프레임에 적이 공격을 내보낸 기록. LateUpdate 마지막에 비운다
        private readonly List<EnemyAttackCommittedRecord> _enemyAttackCommittedRecords = new List<EnemyAttackCommittedRecord>();
        // 이번 프레임의 투사체 명중·폭발 기록. LateUpdate 마지막에 비운다
        private readonly List<ProjectileHitRecord> _projectileHitRecords = new List<ProjectileHitRecord>();
        // 이번 프레임에 적이 피해를 받은 기록. LateUpdate 마지막에 비운다
        private readonly List<EnemyDamagedRecord> _enemyDamagedRecords = new List<EnemyDamagedRecord>();
        // 이번 프레임에 전리품을 획득한 기록. LateUpdate 마지막에 비운다
        private readonly List<LootPickedUpRecord> _lootPickedUpRecords = new List<LootPickedUpRecord>();
    }
}
