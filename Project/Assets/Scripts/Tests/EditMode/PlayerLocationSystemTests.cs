using NUnit.Framework;
using TopView.Components;
using TopView.Entities;
using TopView.Systems;
using UnityEngine;

namespace TopView.Tests.EditMode
{
    // 적이 쫓을 플레이어 위치에 살아 있는 플레이어만 기록하는지 확인한다.
    public class PlayerLocationSystemTests
    {
        private EntityStorage _storage;
        private PlayerLocationComponent _playerLocation;

        private const int PLAYER_INDEX = 0;

        /// <summary>
        /// 플레이어 구간에 칸이 하나 있는 빈 저장소를 만든다.
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            _storage = new EntityStorage(0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0);
            _playerLocation = default;
        }

        /// <summary>
        /// 살아 있는 플레이어의 위치를 기록한다.
        /// </summary>
        [Test]
        public void RecordsAlivePlayer()
        {
            PlacePlayer(PlayerState.Idle);

            PlayerLocationSystem.Tick(_storage, ref _playerLocation);

            Assert.IsTrue(_playerLocation.hasPlayer);
        }

        /// <summary>
        /// 사망한 플레이어는 기록하지 않는다. 기록하면 적이 시체를 쫓아가 공격한다.
        /// </summary>
        [Test]
        public void IgnoresDeadPlayer()
        {
            PlacePlayer(PlayerState.Die);

            PlayerLocationSystem.Tick(_storage, ref _playerLocation);

            Assert.IsFalse(_playerLocation.hasPlayer, "사망한 플레이어를 적이 쫓을 위치로 기록했다.");
        }

        private void PlacePlayer(PlayerState state)
        {
            ref Entity player = ref _storage.entities[PLAYER_INDEX];
            player.mask = ComponentMask.PlayerActionState | ComponentMask.Position | ComponentMask.Body;
            player.generation = 1;
            player.playerActionState.state = state;
            player.position = new PositionComponent { previous = Vector3.one, current = Vector3.one };
            player.body = new BodyComponent { radius = 0.5f };
        }
    }
}
