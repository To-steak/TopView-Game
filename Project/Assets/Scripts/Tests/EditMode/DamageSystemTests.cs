using System.Collections.Generic;
using NUnit.Framework;
using TopView.Components;
using TopView.Entities;
using TopView.Records;
using TopView.Requests;
using TopView.Systems;

namespace TopView.Tests.EditMode
{
    // 일시정지 중에는 피해가 체력에 반영되지 않고, 재개한 뒤에 반영되는지 확인한다.
    public class DamageSystemTests
    {
        private EntityStorage _storage;
        private GameProgressComponent _progress;
        private Queue<DamageRequest> _playerDamageRequests;
        private Queue<DamageRequest> _enemyDamageRequests;
        private List<EnemyDamagedRecord> _enemyDamagedRecords;

        // 저장소의 칸 번호. 구간 순서(적, 플레이어)대로 칸을 하나씩 둔다
        private const int ENEMY_INDEX = 0;
        private const int PLAYER_INDEX = 1;
        private const float MAX_HEALTH = 100f;
        private const float DAMAGE = 30f;

        /// <summary>
        /// 적과 플레이어 구간에 칸이 하나씩 있는 저장소에 둘을 놓고, 진행 중인 판과 빈 큐를 만든다.
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            _storage = new EntityStorage(0, 0, 0, 0, 0, 1, 1, 0, 1, 0, 0, 0);
            _progress = new GameProgressComponent { phase = GamePhase.Playing };
            _playerDamageRequests = new Queue<DamageRequest>();
            _enemyDamageRequests = new Queue<DamageRequest>();
            _enemyDamagedRecords = new List<EnemyDamagedRecord>();

            ref Entity enemy = ref _storage.entities[ENEMY_INDEX];
            enemy.mask = ComponentMask.EnemyKind | ComponentMask.Health | ComponentMask.Position | ComponentMask.Score | ComponentMask.Combat;
            enemy.generation = 1;
            enemy.health = new HealthComponent { current = MAX_HEALTH, max = MAX_HEALTH };

            ref Entity player = ref _storage.entities[PLAYER_INDEX];
            player.mask = ComponentMask.PlayerActionState | ComponentMask.Health;
            player.generation = 1;
            player.health = new HealthComponent { current = MAX_HEALTH, max = MAX_HEALTH };
        }

        /// <summary>
        /// 진행 중이고 일시정지가 아니면 피해를 체력에 반영한다.
        /// </summary>
        [Test]
        public void AppliesDamageWhilePlaying()
        {
            EnqueueDamage();

            DamageSystem.Tick(ref _progress, _storage, _playerDamageRequests, _enemyDamageRequests, _enemyDamagedRecords);

            Assert.AreEqual(MAX_HEALTH - DAMAGE, _storage.entities[ENEMY_INDEX].health.current);
            Assert.AreEqual(MAX_HEALTH - DAMAGE, _storage.entities[PLAYER_INDEX].health.current);
        }

        /// <summary>
        /// 일시정지 중에는 체력을 바꾸지 않고 피해 기록도 남기지 않는다. 사망 판정 System이 일시정지 중에 돌지 않아, 여기서 체력을 깎으면 사망 판정 없이 체력만 0이 된다.
        /// </summary>
        [Test]
        public void KeepsHealthWhilePaused()
        {
            _progress.isPaused = true;
            EnqueueDamage();

            DamageSystem.Tick(ref _progress, _storage, _playerDamageRequests, _enemyDamageRequests, _enemyDamagedRecords);

            Assert.AreEqual(MAX_HEALTH, _storage.entities[ENEMY_INDEX].health.current, "일시정지 중에 적 체력이 바뀌었다.");
            Assert.AreEqual(MAX_HEALTH, _storage.entities[PLAYER_INDEX].health.current, "일시정지 중에 플레이어 체력이 바뀌었다.");
            Assert.AreEqual(0, _enemyDamagedRecords.Count, "일시정지 중에 피해 기록이 남았다.");
        }

        /// <summary>
        /// 일시정지 중에 들어온 피해는 버리지 않고 남겨 두었다가, 재개한 틱에 반영한다.
        /// </summary>
        [Test]
        public void AppliesHeldDamageAfterResume()
        {
            _progress.isPaused = true;
            EnqueueDamage();
            DamageSystem.Tick(ref _progress, _storage, _playerDamageRequests, _enemyDamageRequests, _enemyDamagedRecords);

            _progress.isPaused = false;
            DamageSystem.Tick(ref _progress, _storage, _playerDamageRequests, _enemyDamageRequests, _enemyDamagedRecords);

            Assert.AreEqual(MAX_HEALTH - DAMAGE, _storage.entities[ENEMY_INDEX].health.current);
            Assert.AreEqual(MAX_HEALTH - DAMAGE, _storage.entities[PLAYER_INDEX].health.current);
        }

        private void EnqueueDamage()
        {
            _enemyDamageRequests.Enqueue(new DamageRequest { target = new EntityIdentifier { index = ENEMY_INDEX, generation = 1 }, amount = DAMAGE });
            _playerDamageRequests.Enqueue(new DamageRequest { target = new EntityIdentifier { index = PLAYER_INDEX, generation = 1 }, amount = DAMAGE });
        }
    }
}
