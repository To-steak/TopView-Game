using System.Collections.Generic;
using NUnit.Framework;
using TopView.Components;
using TopView.Entities;
using TopView.Requests;
using TopView.Systems;

namespace TopView.Tests.EditMode
{
    // 사망 연출이 끝난 적이 드롭 표에서 뽑은 전리품을 알맞게 요청하는지 확인한다.
    public class EnemyDeathEndSystemTests
    {
        private EntityStorage _storage;
        private Queue<GameOverRequest> _gameOverRequests;
        private Queue<LootSpawnRequest> _lootSpawnRequests;
        private Queue<EnemyDespawnRequest> _despawnRequests;

        private const int ENEMY_INDEX = 0;
        private const int BASE_AMOUNT = 20;
        private const int AMOUNT_INCREMENT = -3;

        /// <summary>
        /// 적 종류 하나, 칸 하나가 있는 저장소와 빈 요청 큐를 만든다.
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            _storage = new EntityStorage(0, 0, 0, 0, 0, 1, 1, 1, 0, 0, 0, 0);
            _gameOverRequests = new Queue<GameOverRequest>();
            _lootSpawnRequests = new Queue<LootSpawnRequest>();
            _despawnRequests = new Queue<EnemyDespawnRequest>();
        }

        /// <summary>
        /// 뽑힌 전리품의 양이 0 이하(20 + 7 × -3 = -1)면 전리품을 떨어뜨리지 않는다. 음수 체력 전리품을 주우면 체력이 준다.
        /// </summary>
        [Test]
        public void SkipsLootWithNonPositiveAmount()
        {
            PlaceDeadEnemy(7);

            EnemyDeathEndSystem.Tick(_storage, _gameOverRequests, new System.Random(0), _lootSpawnRequests, _despawnRequests);

            Assert.AreEqual(1, _despawnRequests.Count, "준비 단계: 사망 연출이 끝난 적의 소멸을 요청하지 않았다.");
            Assert.AreEqual(0, _lootSpawnRequests.Count, $"양이 {(_lootSpawnRequests.Count > 0 ? _lootSpawnRequests.Peek().amount : 0)}인 전리품의 생성을 요청했다. 양이 0 이하인 전리품은 떨어뜨리면 안 된다.");
        }

        /// <summary>
        /// 대조: 양이 양수(20 + 0 × -3 = 20)면 그 양으로 전리품의 생성을 요청한다.
        /// </summary>
        [Test]
        public void SpawnsLootWithPositiveAmount()
        {
            PlaceDeadEnemy(0);

            EnemyDeathEndSystem.Tick(_storage, _gameOverRequests, new System.Random(0), _lootSpawnRequests, _despawnRequests);

            Assert.AreEqual(1, _lootSpawnRequests.Count);
            Assert.AreEqual(BASE_AMOUNT, _lootSpawnRequests.Peek().amount);
        }

        // 드롭 표에 체력 전리품 하나만 있는, 사망 연출이 끝난 적을 놓는다
        private void PlaceDeadEnemy(int level)
        {
            ref Entity enemy = ref _storage.entities[ENEMY_INDEX];
            enemy.mask = ComponentMask.Position | ComponentMask.EnemyKind | ComponentMask.EnemyActionState | ComponentMask.Death | ComponentMask.LootList | ComponentMask.Combat;
            enemy.generation = 1;
            enemy.enemyActionState.state = EnemyState.Die;
            enemy.enemyActionState.elapsedTicks = 10;
            enemy.death = new DeathComponent { dieTicks = 10 };
            enemy.combat.level = level;
            enemy.lootList.loots = new EnemyLootComponent[]
            {
                new EnemyLootComponent { kind = LootKind.Health, weight = 1, baseAmount = BASE_AMOUNT, amountIncrement = AMOUNT_INCREMENT }
            };
        }
    }
}
