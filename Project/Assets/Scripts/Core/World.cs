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
        // 웨이브 목록. 판 시작 때 Definition에서 복사한다
        public WaveComponent[] waves;
        // 적 종류별 생기는 지점 목록. 판 시작 때 Spawner Authoring에서 채운다. 첫 번호가 적 종류 번호다
        public SpawnPointComponent[][] enemySpawnPointsByKind;
        // 한 판의 진행 상태
        public GameProgressComponent progress;
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
        // 보스 저장소
        public BossStorage bosses;
        // 투사체 저장소
        public ProjectileStorage projectiles;

        // 판을 준비할 때 종류별로 Definition 값을 채워 둔 적 생성 요청. 번호가 적 종류 번호다
        public EnemySpawnRequest[] enemySpawnTemplates;
        // 판을 준비할 때 Definition 값과 Boss Spawner Authoring의 위치로 채워 둔 보스 생성 요청
        public BossSpawnRequest bossSpawnTemplate;

        // 플레이어 생성 요청 큐
        public Queue<PlayerSpawnRequest> playerSpawnRequests = new Queue<PlayerSpawnRequest>();
        // 적 생성 요청 큐
        public Queue<EnemySpawnRequest> enemySpawnRequests = new Queue<EnemySpawnRequest>();
        // 보스 생성 요청 큐
        public Queue<BossSpawnRequest> bossSpawnRequests = new Queue<BossSpawnRequest>();
        // 투사체 생성 요청 큐
        public Queue<ProjectileSpawnRequest> projectileSpawnRequests = new Queue<ProjectileSpawnRequest>();
        // 피해 요청 큐
        public Queue<DamageRequest> damageRequests = new Queue<DamageRequest>();
        // 게임오버 요청 큐
        public Queue<GameOverRequest> gameOverRequests = new Queue<GameOverRequest>();

        // 이번 프레임의 플레이어 상태 변경 기록. LateUpdate 마지막에 비운다
        public List<PlayerStateChangedRecord> playerStateChangedRecords = new List<PlayerStateChangedRecord>();
        // 이번 프레임의 적 상태 변경 기록. LateUpdate 마지막에 비운다
        public List<EnemyStateChangedRecord> enemyStateChangedRecords = new List<EnemyStateChangedRecord>();
        // 이번 프레임의 보스 상태 변경 기록. LateUpdate 마지막에 비운다
        public List<BossStateChangedRecord> bossStateChangedRecords = new List<BossStateChangedRecord>();
        // 이번 프레임의 투사체 명중·폭발 기록. LateUpdate 마지막에 비운다
        public List<ProjectileHitRecord> projectileHitRecords = new List<ProjectileHitRecord>();
    }
}
