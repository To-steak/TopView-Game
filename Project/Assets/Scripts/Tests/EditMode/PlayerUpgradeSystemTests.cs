using System.Collections.Generic;
using NUnit.Framework;
using TopView.Commands;
using TopView.Components;
using TopView.Entities;
using TopView.Systems;

namespace TopView.Tests.EditMode
{
    // 강화가 살아 있는 플레이어에게만 적용되는지, 무기 강화로 늘어난 탄약을 바로 넣는지 확인한다.
    public class PlayerUpgradeSystemTests
    {
        private EntityStorage _storage;
        private GameProgressComponent _progress;
        private List<PlayerUpgradeCommand> _commands;

        private const int PLAYER_INDEX = 0;
        private const int WEAPON_INDEX = 1;
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

        /// <summary>
        /// 무기를 0레벨에서 1레벨로 올려 최대 탄약 수가 3 × 1.5 = 4.5가 되면, 사사오입한 5발에 맞춰 늘어난 2발을 탄창에 넣는다.
        /// Mathf.RoundToInt는 .5를 짝수로 보내 4발이 되므로 1발만 늘어난다.
        /// </summary>
        [Test]
        public void RoundsHalfMaxAmmoUp()
        {
            PlacePlayerWithWeapon(3, 0.5f);

            PlayerUpgradeSystem.Tick(_storage, ref _progress, _commands);

            Assert.AreEqual(5, _storage.entities[WEAPON_INDEX].magazineState.current, "최대 탄약 4.5발을 사사오입하지 않고 짝수 쪽(4발)으로 반올림해, 강화로 늘어난 탄약이 2발이 아니라 1발이다.");
        }

        /// <summary>
        /// 대조: 최대 탄약 수가 .5로 끝나지 않으면(4 × 1.5 = 6) 늘어난 만큼 정확히 탄창에 넣는다.
        /// </summary>
        [Test]
        public void FillsAddedAmmoWhenMaxAmmoIsWhole()
        {
            PlacePlayerWithWeapon(4, 0.5f);

            PlayerUpgradeSystem.Tick(_storage, ref _progress, _commands);

            Assert.AreEqual(6, _storage.entities[WEAPON_INDEX].magazineState.current);
        }

        // 플레이어 칸 하나와 무기 칸 하나가 있는 저장소에, 탄창이 가득 찬 0레벨 무기를 든 플레이어를 놓고 무기 강화 명령을 넣는다
        private void PlacePlayerWithWeapon(int maxAmmo, float maxAmmoRatioPerLevel)
        {
            _storage = new EntityStorage(0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 0, 0);
            PlacePlayer(PlayerState.Idle, MAX_HEALTH);
            _storage.entities[PLAYER_INDEX].weaponSlots.weapons = new EntityIdentifier[] { new EntityIdentifier { index = WEAPON_INDEX, generation = 1 } };

            ref Entity weapon = ref _storage.entities[WEAPON_INDEX];
            weapon.mask = ComponentMask.UpgradeLevel | ComponentMask.Magazine | ComponentMask.MagazineState | ComponentMask.WeaponUpgrade;
            weapon.generation = 1;
            weapon.upgradeLevel.level = 0;
            weapon.magazine = new MagazineComponent { max = maxAmmo, startingCurrent = maxAmmo };
            weapon.magazineState.current = maxAmmo;
            weapon.weaponUpgrade = new WeaponUpgradeRuleComponent { maxAmmoRatioPerLevel = maxAmmoRatioPerLevel };

            _commands[0] = new PlayerUpgradeCommand { type = PlayerUpgradeCommandType.Weapon, weaponIndex = 0 };
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
