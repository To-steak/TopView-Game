using System.Collections.Generic;
using TopView.Commands;
using TopView.Components;
using TopView.Entities;
using TopView.Records;
using TopView.Requests;
using UnityEngine.AI;

namespace TopView.Core
{
    // 한 판의 데이터. Runner만 쓰며, 판마다 새로 만든다.
    public class World
    {
        // 게임 규칙 값. 판 시작 때 Definition에서 복사한다
        public GameRuleComponent rule;
        // 전리품 규칙 값. 판 시작 때 Definition에서 복사한다
        public LootRuleComponent lootRule;
        // 적이 생기는 지점 목록. 판 시작 때 켜진 Spawner Authoring에서 채운다
        public SpawnPointComponent[] enemySpawnPoints;
        // 한 판의 진행 상태
        public GameProgressComponent progress;
        // 이번 틱에 적이 노리는 플레이어의 위치. 매 틱 PlayerLocationSystem이 다시 채운다
        public PlayerLocationComponent playerLocation;
        // 시드 고정 난수 생성기
        public System.Random random;
        // 입력 명령 버퍼
        public CommandBuffer commandBuffer = new CommandBuffer();
        // 적 경로 계산에 다시 쓰는 NavMeshPath
        public NavMeshPath navigationPath = new NavMeshPath();

        // 플레이어 저장소
        public PlayerStorage players;
        // 적 저장소
        public EnemyStorage enemies;
        // 근접 판정 저장소
        public MeleeStorage melees;
        // 투사체 저장소
        public ProjectileStorage projectiles;
        // 전리품 저장소
        public LootStorage loots;
        // 이번 틱의 플레이어 피격 대상 목록. 적의 공격이 읽는다. 판 시작 때 플레이어 칸 수로 만들고, 매 틱 HitTargetSystem이 다시 채운다
        public HitTargetComponent[] playerHitTargets;
        // 플레이어 피격 대상 목록에서 이번 틱에 채운 대상 수
        public int playerHitTargetCount;
        // 이번 틱의 적 피격 대상 목록. 플레이어의 공격이 읽는다. 판 시작 때 적 칸 수로 만들고, 매 틱 HitTargetSystem이 다시 채운다
        public HitTargetComponent[] enemyHitTargets;
        // 적 피격 대상 목록에서 이번 틱에 채운 대상 수
        public int enemyHitTargetCount;

        // 판을 준비할 때 종류별로 Definition 값을 채워 둔 적 생성 요청. 번호가 적 종류 번호다
        public EnemySpawnRequest[] enemySpawnTemplates;

        // 플레이어 생성 요청 큐
        public Queue<PlayerSpawnRequest> playerSpawnRequests = new Queue<PlayerSpawnRequest>();
        // 적 생성 요청 큐
        public Queue<EnemySpawnRequest> enemySpawnRequests = new Queue<EnemySpawnRequest>();
        // 투사체 생성 요청 큐
        public Queue<ProjectileSpawnRequest> projectileSpawnRequests = new Queue<ProjectileSpawnRequest>();
        // 근접 판정 생성 요청 큐
        public Queue<MeleeSpawnRequest> meleeSpawnRequests = new Queue<MeleeSpawnRequest>();
        // 전리품 생성 요청 큐
        public Queue<LootSpawnRequest> lootSpawnRequests = new Queue<LootSpawnRequest>();
        // 플레이어 상태 변경 요청 큐
        public Queue<PlayerStateChangeRequest> playerStateChangeRequests = new Queue<PlayerStateChangeRequest>();
        // 적 상태 변경 요청 큐
        public Queue<EnemyStateChangeRequest> enemyStateChangeRequests = new Queue<EnemyStateChangeRequest>();
        // 투사체 소멸 요청 큐
        public Queue<ProjectileDespawnRequest> projectileDespawnRequests = new Queue<ProjectileDespawnRequest>();
        // 근접 판정 소멸 요청 큐
        public Queue<MeleeDespawnRequest> meleeDespawnRequests = new Queue<MeleeDespawnRequest>();
        // 전리품 소멸 요청 큐
        public Queue<LootDespawnRequest> lootDespawnRequests = new Queue<LootDespawnRequest>();
        // 플레이어 피해 요청 큐. 적의 공격이 넣는다
        public Queue<DamageRequest> playerDamageRequests = new Queue<DamageRequest>();
        // 적 피해 요청 큐. 플레이어의 공격이 넣는다
        public Queue<DamageRequest> enemyDamageRequests = new Queue<DamageRequest>();
        // 게임오버 요청 큐
        public Queue<GameOverRequest> gameOverRequests = new Queue<GameOverRequest>();

        // 이번 프레임의 플레이어 상태 변경 기록. LateUpdate 마지막에 비운다
        public List<PlayerStateChangedRecord> playerStateChangedRecords = new List<PlayerStateChangedRecord>();
        // 이번 프레임의 적 상태 변경 기록. LateUpdate 마지막에 비운다
        public List<EnemyStateChangedRecord> enemyStateChangedRecords = new List<EnemyStateChangedRecord>();
        // 이번 프레임의 투사체 명중·폭발 기록. LateUpdate 마지막에 비운다
        public List<ProjectileHitRecord> projectileHitRecords = new List<ProjectileHitRecord>();
        // 이번 프레임에 적이 피해를 받은 기록. LateUpdate 마지막에 비운다
        public List<EnemyDamagedRecord> enemyDamagedRecords = new List<EnemyDamagedRecord>();
    }
}
