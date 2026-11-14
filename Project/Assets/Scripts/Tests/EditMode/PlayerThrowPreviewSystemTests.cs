using NUnit.Framework;
using TopView.Components;
using TopView.Entities;
using TopView.Systems;
using UnityEngine;

namespace TopView.Tests.EditMode
{
    // 투척 미리보기가 실제로 던질 수류탄과 같은 폭발 반지름을 보여 주는지 확인한다.
    public class PlayerThrowPreviewSystemTests
    {
        private EntityStorage _storage;

        // 저장소의 칸 번호. 구간 순서(플레이어, 무기)대로 칸을 하나씩 둔다
        private const int PLAYER_INDEX = 0;
        private const int WEAPON_INDEX = 1;
        private const int FLIGHT_TICKS = 10;
        private const float BASE_EXPLOSION_RADIUS = 2f;
        private const float EXPLOSION_RADIUS_INCREASE_PER_LEVEL = 0.5f;

        /// <summary>
        /// 플레이어와 무기 구간에 칸이 하나씩 있는 빈 저장소를 만든다.
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            _storage = new EntityStorage(0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 0, 0);
        }

        /// <summary>
        /// 강화 레벨이 0이면 미리보기의 폭발 반지름은 무기의 기본 반지름이다.
        /// </summary>
        [Test]
        public void ShowsBaseExplosionRadiusAtLevelZero()
        {
            PlaceChargingPlayer();
            PlaceThrowWeapon(0);

            PlayerThrowPreviewSystem.Tick(_storage);

            ref readonly PlayerThrowPreviewComponent preview = ref _storage.entities[PLAYER_INDEX].throwPreview;
            Assert.IsTrue(preview.isVisible);
            Assert.AreEqual(BASE_EXPLOSION_RADIUS, preview.explosionRadius, 0.0001f);
        }

        /// <summary>
        /// 강화 레벨이 오르면 미리보기의 폭발 반지름도 실제 수류탄(PlayerArcAttackSystem)처럼 레벨만큼 커진다.
        /// </summary>
        [Test]
        public void ShowsUpgradedExplosionRadius()
        {
            PlaceChargingPlayer();
            PlaceThrowWeapon(2);

            PlayerThrowPreviewSystem.Tick(_storage);

            float expectedRadius = BASE_EXPLOSION_RADIUS + EXPLOSION_RADIUS_INCREASE_PER_LEVEL * 2;
            Assert.AreEqual(expectedRadius, _storage.entities[PLAYER_INDEX].throwPreview.explosionRadius, 0.0001f);
        }

        private void PlaceChargingPlayer()
        {
            ref Entity player = ref _storage.entities[PLAYER_INDEX];
            player.mask = ComponentMask.PlayerActionState | ComponentMask.Position | ComponentMask.Rotation | ComponentMask.WeaponSlots | ComponentMask.ThrowPreview;
            player.generation = 1;
            player.playerActionState.state = PlayerState.ThrowCharge;
            player.rotation.current = Quaternion.identity;
            player.weaponSlots.weapons = new EntityIdentifier[] { new EntityIdentifier { index = WEAPON_INDEX, generation = 1 } };
            player.weaponSlots.currentIndex = 0;
            player.throwPreview.points = new Vector3[FLIGHT_TICKS + 1];
        }

        private void PlaceThrowWeapon(int level)
        {
            ref Entity weapon = ref _storage.entities[WEAPON_INDEX];
            weapon.mask = ComponentMask.ArcAttack | ComponentMask.Throw | ComponentMask.UpgradeLevel | ComponentMask.WeaponUpgrade;
            weapon.generation = 1;
            weapon.arcAttack.projectile.flight.flightTicks = FLIGHT_TICKS;
            weapon.arcAttack.projectile.flight.arcHeight = 1f;
            weapon.arcAttack.projectile.explosion.radius = BASE_EXPLOSION_RADIUS;
            weapon.arcAttack.shots = new ProjectileShotComponent[] { new ProjectileShotComponent() };
            weapon.throwing.maxThrowDistance = 5f;
            weapon.upgradeLevel.level = level;
            weapon.weaponUpgrade.explosionRadiusIncreasePerLevel = EXPLOSION_RADIUS_INCREASE_PER_LEVEL;
        }
    }
}
