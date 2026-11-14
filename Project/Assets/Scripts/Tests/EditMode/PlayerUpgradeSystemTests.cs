using System.Collections.Generic;
using NUnit.Framework;
using TopView.Commands;
using TopView.Components;
using TopView.Entities;
using TopView.Systems;

namespace TopView.Tests.EditMode
{
    // 강화가 살아 있는 플레이어에게만 적용되는지 확인한다.
    public class PlayerUpgradeSystemTests
    {
        private EntityStorage _storage;
        private GameProgressComponent _progress;
        private List<PlayerUpgradeCommand> _commands;

        private const int PLAYER_INDEX = 0;
        private const float MAX_HEALTH = 100f;
        private const int GOLD = 1000;
        private const int COST = 10;

        /// <summary>
        /// 플레이어 구간에 칸이 하나 있는 저장소와, 강화 화면이 열리고 골드가 충분한 판을 만든다.
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            _storage = new EntityStorage(0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0);
            _progress = new GameProgressComponent { phase = GamePhase.Playing, isPaused = true, isUpgradeOpen = true, gold = GOLD };
            _commands = new List<PlayerUpgradeCommand> { new PlayerUpgradeCommand { type = PlayerUpgradeCommandType.MaxHealth } };
        }

        /// <summary>
        /// 살아 있는 플레이어의 최대 체력을 올리고, 늘어난 만큼 체력을 채운다.
        /// </summary>
        [Test]
        public void UpgradesAlivePlayer()
        {
            PlacePlayer(PlayerState.Idle, MAX_HEALTH);

            PlayerUpgradeSystem.Tick(_storage, ref _progress, _commands);

            Assert.Greater(_storage.entities[PLAYER_INDEX].health.max, MAX_HEALTH);
            Assert.AreEqual(GOLD - COST, _progress.gold);
        }

        /// <summary>
        /// 사망한 플레이어는 강화하지 않는다. 최대 체력 강화가 체력 0인 플레이어의 체력을 다시 채우면 안 된다.
        /// </summary>
        [Test]
        public void IgnoresDeadPlayer()
        {
            PlacePlayer(PlayerState.Die, 0f);

            PlayerUpgradeSystem.Tick(_storage, ref _progress, _commands);

            Assert.AreEqual(0f, _storage.entities[PLAYER_INDEX].health.current, "사망한 플레이어의 체력이 강화로 다시 채워졌다.");
            Assert.AreEqual(GOLD, _progress.gold, "사망한 플레이어의 강화에 골드를 썼다.");
        }

        private void PlacePlayer(PlayerState state, float health)
        {
            ref Entity player = ref _storage.entities[PLAYER_INDEX];
            player.mask = ComponentMask.PlayerActionState | ComponentMask.Health | ComponentMask.Mana | ComponentMask.PlayerVitals
                | ComponentMask.PlayerUpgrade | ComponentMask.PlayerUpgradeLevels | ComponentMask.WeaponSlots;
            player.generation = 1;
            player.playerActionState.state = state;
            player.health = new HealthComponent { current = health, max = MAX_HEALTH };
            player.playerVitals = new PlayerVitalsComponent { startingMaxHealth = MAX_HEALTH };
            player.playerUpgrade = new PlayerUpgradeComponent { maxLevel = 5, startingCost = COST, maxHealthRatioPerLevel = 0.1f };
            player.weaponSlots.weapons = new EntityIdentifier[0];
        }
    }
}
