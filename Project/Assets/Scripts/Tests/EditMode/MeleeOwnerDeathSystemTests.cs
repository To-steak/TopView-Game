using System.Collections.Generic;
using NUnit.Framework;
using TopView.Entities;
using TopView.Requests;
using TopView.Systems;

namespace TopView.Tests.EditMode
{
    // 근접 판정의 주인이 사망 상태가 되면 그 판정의 소멸을 요청하는지 확인한다.
    public class MeleeOwnerDeathSystemTests
    {
        private EntityStorage _storage;
        private Queue<MeleeDespawnRequest> _despawnRequests;

        // 저장소의 칸 번호. 구간 순서(근접 판정, 적, 플레이어)대로 칸을 하나씩 둔다
        private const int MELEE_INDEX = 0;
        private const int ENEMY_INDEX = 1;
        private const int PLAYER_INDEX = 2;

        /// <summary>
        /// 근접 판정, 적, 플레이어 구간에 칸이 하나씩 있는 빈 저장소와 빈 소멸 요청 큐를 만든다.
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            _storage = new EntityStorage(0, 1, 1, 0, 0, 1, 1, 1, 1, 0, 0, 0);
            _despawnRequests = new Queue<MeleeDespawnRequest>();
        }

        /// <summary>
        /// 주인인 플레이어가 사망 상태면 근접 판정의 소멸을 요청한다.
        /// </summary>
        [Test]
        public void RequestsDespawnWhenPlayerOwnerIsDead()
        {
            PlacePlayer(PlayerState.Die);
            PlaceMelee(PLAYER_INDEX);

            MeleeOwnerDeathSystem.Tick(_storage, _despawnRequests);

            AssertDespawnRequested();
        }

        /// <summary>
        /// 주인인 적이 사망 상태면 근접 판정의 소멸을 요청한다. 죽은 적의 공격에 맞지 않게 하기 위해서다.
        /// </summary>
        [Test]
        public void RequestsDespawnWhenEnemyOwnerIsDead()
        {
            PlaceEnemy(EnemyState.Die);
            PlaceMelee(ENEMY_INDEX);

            MeleeOwnerDeathSystem.Tick(_storage, _despawnRequests);

            AssertDespawnRequested();
        }

        /// <summary>
        /// 주인인 적이 살아 있으면 근접 판정을 그대로 둔다.
        /// </summary>
        [Test]
        public void KeepsMeleeWhenEnemyOwnerIsAlive()
        {
            PlaceEnemy(EnemyState.Attack);
            PlaceMelee(ENEMY_INDEX);

            MeleeOwnerDeathSystem.Tick(_storage, _despawnRequests);

            Assert.AreEqual(0, _despawnRequests.Count);
        }

        private void PlacePlayer(PlayerState state)
        {
            ref Entity player = ref _storage.entities[PLAYER_INDEX];
            player.mask = ComponentMask.PlayerActionState;
            player.generation = 1;
            player.playerActionState.state = state;
        }

        private void PlaceEnemy(EnemyState state)
        {
            ref Entity enemy = ref _storage.entities[ENEMY_INDEX];
            enemy.mask = ComponentMask.EnemyActionState;
            enemy.generation = 1;
            enemy.enemyActionState.state = state;
        }

        private void PlaceMelee(int ownerIndex)
        {
            ref Entity melee = ref _storage.entities[MELEE_INDEX];
            melee.mask = ComponentMask.Melee;
            melee.generation = 1;
            melee.melee.owner = new EntityIdentifier { index = ownerIndex, generation = 1 };
        }

        private void AssertDespawnRequested()
        {
            Assert.AreEqual(1, _despawnRequests.Count);
            MeleeDespawnRequest request = _despawnRequests.Peek();
            Assert.AreEqual(MELEE_INDEX, request.target.index);
            Assert.AreEqual(1, request.target.generation);
        }
    }
}
