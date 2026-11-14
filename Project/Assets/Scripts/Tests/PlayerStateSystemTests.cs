using System.Collections.Generic;
using NUnit.Framework;
using TopView.Commands;
using TopView.Entities;
using TopView.Requests;
using TopView.Systems;

namespace TopView.Tests
{
    // 사망한 플레이어가 사망 시간을 채우면 패배를 요청하는지 확인한다.
    public class PlayerStateSystemTests
    {
        private EntityStorage _storage;
        private Queue<PlayerStateChangeRequest> _stateChangeRequests;
        private Queue<GameOverRequest> _gameOverRequests;
        private PlayerCommand _command;

        // 저장소의 칸 번호. 구간 순서(플레이어, 무기)대로 칸을 하나씩 둔다
        private const int PLAYER_INDEX = 0;
        private const int WEAPON_INDEX = 1;

        /// <summary>
        /// 플레이어와 무기 구간에 칸이 하나씩 있는 빈 저장소와 빈 큐, 입력 없는 명령을 만든다.
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            _storage = new EntityStorage(0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 0, 0);
            _stateChangeRequests = new Queue<PlayerStateChangeRequest>();
            _gameOverRequests = new Queue<GameOverRequest>();
            _command = new PlayerCommand();
        }

        /// <summary>
        /// 사망 시간을 채우기 전에는 패배를 요청하지 않는다.
        /// </summary>
        [Test]
        public void WaitsUntilDieTicks()
        {
            PlaceDeadPlayer(5, 4);

            PlayerStateSystem.Tick(_storage, in _command, _stateChangeRequests, _gameOverRequests);

            Assert.AreEqual(0, _gameOverRequests.Count);
        }

        /// <summary>
        /// 사망 시간을 채운 틱에 패배를 요청한다.
        /// </summary>
        [Test]
        public void RequestsDefeatAtDieTicks()
        {
            PlaceDeadPlayer(5, 5);

            PlayerStateSystem.Tick(_storage, in _command, _stateChangeRequests, _gameOverRequests);

            AssertDefeatRequested();
        }

        /// <summary>
        /// 사망 시간이 0이어도 패배를 요청한다. 사망 상태의 경과 틱은 PlayerStateTimerSystem을 거쳐 1부터 읽힌다.
        /// </summary>
        [Test]
        public void RequestsDefeatWhenDieTicksIsZero()
        {
            PlaceDeadPlayer(0, 1);

            PlayerStateSystem.Tick(_storage, in _command, _stateChangeRequests, _gameOverRequests);

            AssertDefeatRequested();
        }

        private void PlaceDeadPlayer(int dieTicks, int elapsedTicks)
        {
            ref Entity player = ref _storage.entities[PLAYER_INDEX];
            player.mask = ComponentMask.PlayerActionState | ComponentMask.Death | ComponentMask.PlayerAction | ComponentMask.PlayerMotion | ComponentMask.WeaponSlots;
            player.generation = 1;
            player.playerActionState.state = PlayerState.Die;
            player.playerActionState.elapsedTicks = elapsedTicks;
            player.death.dieTicks = dieTicks;
            player.death.isGameOverOnDeath = true;
            player.weaponSlots.weapons = new EntityIdentifier[] { new EntityIdentifier { index = WEAPON_INDEX, generation = 1 } };
        }

        private void AssertDefeatRequested()
        {
            Assert.AreEqual(1, _gameOverRequests.Count);
            Assert.IsFalse(_gameOverRequests.Peek().isVictory);
        }
    }
}
