using System.Collections.Generic;
using NUnit.Framework;
using TopView.Entities;
using TopView.Requests;
using TopView.Systems;
using UnityEngine;
using UnityEngine.TestTools;

namespace TopView.Tests
{
    // 적 스포너가 실제로 만들 수 있는 적만 만든 것으로 세는지 확인한다.
    public class EnemySpawnerSystemTests
    {
        private EntityStorage _storage;
        private EnemySpawnRequest[] _enemySpawnTemplates;
        private Queue<EnemySpawnerActivationRequest> _activationRequests;
        private Queue<EnemySpawnRequest> _enemySpawnRequests;

        private const int ENEMY_KIND_INDEX = 0;

        /// <summary>
        /// 적 종류 하나의 생성 요청 틀과 빈 큐를 만든다. 저장소는 테스트마다 적 칸 수를 정해 만든다.
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            _enemySpawnTemplates = new EnemySpawnRequest[] { new EnemySpawnRequest { enemyKindIndex = ENEMY_KIND_INDEX } };
            _activationRequests = new Queue<EnemySpawnerActivationRequest>();
            _enemySpawnRequests = new Queue<EnemySpawnRequest>();
        }

        /// <summary>
        /// 그 종류의 빈 칸이 있으면 적 생성 요청을 넣고 한 마리를 만든 것으로 센다.
        /// </summary>
        [Test]
        public void SpawnsWhenFreeIndexExists()
        {
            int spawnerIndex = CreateStorage(1);
            PlaceActiveSpawner(spawnerIndex, 1);

            EnemySpawnerSystem.Tick(1, _storage, _enemySpawnTemplates, _activationRequests, _enemySpawnRequests);

            ref readonly Entity spawner = ref _storage.entities[spawnerIndex];
            Assert.AreEqual(1, _enemySpawnRequests.Count);
            Assert.AreEqual(1, spawner.enemySpawnerState.spawnedCount);
            Assert.IsFalse(spawner.enemySpawnerState.isActive);
        }

        /// <summary>
        /// 그 종류의 빈 칸이 없으면 만든 것으로 세지 않고 작동한 채로 남는다. 요청이 EnemySpawnSystem에서 버려지면 보스 같은 한 마리짜리 스포너가 영영 적을 만들지 못한다.
        /// </summary>
        [Test]
        public void WaitsWhenNoFreeIndex()
        {
            int spawnerIndex = CreateStorage(0);
            PlaceActiveSpawner(spawnerIndex, 1);
            LogAssert.Expect(LogType.Warning, new System.Text.RegularExpressions.Regex("EnemySpawnerSystem"));

            EnemySpawnerSystem.Tick(1, _storage, _enemySpawnTemplates, _activationRequests, _enemySpawnRequests);

            ref readonly Entity spawner = ref _storage.entities[spawnerIndex];
            Assert.AreEqual(0, _enemySpawnRequests.Count);
            Assert.AreEqual(0, spawner.enemySpawnerState.spawnedCount);
            Assert.IsTrue(spawner.enemySpawnerState.isActive);
        }

        /// <summary>
        /// 같은 틱에 같은 종류의 스포너 둘이 적을 내보내도 빈 칸 수만큼만 만든다.
        /// </summary>
        [Test]
        public void SpawnsOnlyAsManyAsFreeIndices()
        {
            int firstSpawnerIndex = CreateStorage(1, 2);
            PlaceActiveSpawner(firstSpawnerIndex, 1);
            PlaceActiveSpawner(firstSpawnerIndex + 1, 1);
            LogAssert.Expect(LogType.Warning, new System.Text.RegularExpressions.Regex("EnemySpawnerSystem"));

            EnemySpawnerSystem.Tick(1, _storage, _enemySpawnTemplates, _activationRequests, _enemySpawnRequests);

            Assert.AreEqual(1, _enemySpawnRequests.Count);
            Assert.AreEqual(1, _storage.entities[firstSpawnerIndex].enemySpawnerState.spawnedCount + _storage.entities[firstSpawnerIndex + 1].enemySpawnerState.spawnedCount);
        }

        // 적 종류 하나에 enemyCapacity칸, 적 스포너 spawnerCapacity칸을 둔 저장소를 만들고 첫 스포너 칸 번호를 돌려준다.
        private int CreateStorage(int enemyCapacity, int spawnerCapacity = 1)
        {
            _storage = new EntityStorage(0, 0, 0, 0, 0, 1, enemyCapacity, 0, 0, 0, spawnerCapacity, 0);
            return enemyCapacity;
        }

        private void PlaceActiveSpawner(int index, int maxSpawnCount)
        {
            ref Entity spawner = ref _storage.entities[index];
            spawner.mask = EntityArchetype.ENEMY_SPAWNER;
            spawner.generation = 1;
            spawner.rotation.current = Quaternion.identity;
            spawner.enemySpawner.enemyKindIndex = ENEMY_KIND_INDEX;
            spawner.enemySpawnerRule.maxSpawnCount = maxSpawnCount;
            spawner.enemySpawnerRule.spawnIntervalTicks = 10;
            spawner.enemySpawnerState.isActive = true;
            spawner.enemySpawnerState.remainingTicks = 1;
        }
    }
}
