using System.Collections.Generic;
using TopView.Commands;
using TopView.Components;
using TopView.Definitions;
using TopView.Entities;
using TopView.Presentation.ViewContexts;
using TopView.UserInterface.Presenters;
using TopView.Utilities;
using UnityEngine;

namespace TopView.Core
{
    // UI: 인스펙터에서 연결한 Presenter에 명령 목록을 넘기고, 매 프레임 필요한 데이터만 넘긴다
    public partial class World
    {
        [SerializeField] private GameOverPresenter _gameOverPresenter;
        [SerializeField] private RemainingTimePresenter _remainingTimePresenter;
        [SerializeField] private ScorePresenter _scorePresenter;
        [SerializeField] private GoldPresenter _goldPresenter;
        [SerializeField] private PlayerHealthPresenter _playerHealthPresenter;
        [SerializeField] private AmmoPresenter _ammoPresenter;
        [SerializeField] private PlayerManaPresenter _playerManaPresenter;
        [SerializeField] private ReloadPresenter _reloadPresenter;
        [SerializeField] private BossHealthPresenter _bossHealthPresenter;
        [SerializeField] private PausePresenter _pausePresenter;
        [SerializeField] private UpgradePresenter _upgradePresenter;
        // 설정 화면. 앱 수명의 UI라 World는 연결하지 않고, App에 등록할 때 넘긴다
        [SerializeField] private SettingsPresenter _settingsPresenter;

        // App의 씬과 앱 전환 명령 목록. App이 World를 등록할 때 넘긴다
        private List<SceneCommand> _sceneCommands;
        // App의 설정 명령 목록. App이 World를 등록할 때 넘긴다
        private List<SettingsCommand> _settingsCommands;
        // App의 UI 소리 요청 목록. App이 World를 등록할 때 넘긴다
        private List<UserInterfaceSoundKind> _userInterfaceSoundRequests;

        private void BindPresenters()
        {
            if (_gameOverPresenter == null)
            {
                Debug.LogError("World >> Game Over Presenter가 연결되어 있지 않다.");
            }
            else
            {
                _gameOverPresenter.Initialize(_sceneCommands, _userInterfaceSoundRequests);
            }

            if (_remainingTimePresenter == null)
            {
                Debug.LogError("World >> Remaining Time Presenter가 연결되어 있지 않다.");
            }
            else
            {
                _remainingTimePresenter.Initialize();
            }

            if (_scorePresenter == null)
            {
                Debug.LogError("World >> Score Presenter가 연결되어 있지 않다.");
            }
            else
            {
                _scorePresenter.Initialize();
            }

            if (_goldPresenter == null)
            {
                Debug.LogError("World >> Gold Presenter가 연결되어 있지 않다.");
            }
            else
            {
                _goldPresenter.Initialize();
            }

            if (_playerHealthPresenter == null)
            {
                Debug.LogError("World >> Player Health Presenter가 연결되어 있지 않다.");
            }
            else
            {
                _playerHealthPresenter.Initialize();
            }

            if (_ammoPresenter == null)
            {
                Debug.LogError("World >> Ammo Presenter가 연결되어 있지 않다.");
            }
            else
            {
                _ammoPresenter.Initialize();
            }

            if (_playerManaPresenter == null)
            {
                Debug.LogError("World >> Player Mana Presenter가 연결되어 있지 않다.");
            }
            else
            {
                _playerManaPresenter.Initialize();
            }

            if (_reloadPresenter == null)
            {
                Debug.LogError("World >> Reload Presenter가 연결되어 있지 않다.");
            }
            else
            {
                _reloadPresenter.Initialize();
            }

            if (_bossHealthPresenter == null)
            {
                Debug.LogError("World >> Boss Health Presenter가 연결되어 있지 않다.");
            }
            else
            {
                _bossHealthPresenter.Initialize();
            }

            if (_pausePresenter == null)
            {
                Debug.LogError("World >> Pause Presenter가 연결되어 있지 않다.");
            }
            else
            {
                _pausePresenter.Initialize(_commandBuffer, _sceneCommands, _settingsCommands, _userInterfaceSoundRequests);
            }

            if (_upgradePresenter == null)
            {
                Debug.LogError("World >> Upgrade Presenter가 연결되어 있지 않다.");
            }
            else
            {
                _upgradePresenter.Initialize(_commandBuffer, _userInterfaceSoundRequests);
            }
        }

        private void PresentUserInterface(bool isSettingsOpen)
        {
            GamePhase phase = _progress.phase;
            if (_gameOverPresenter != null)
            {
                _gameOverPresenter.Present(phase, _progress.score);
            }
            if (_remainingTimePresenter != null)
            {
                _remainingTimePresenter.Present(_progress.remainingTicks);
            }
            if (_scorePresenter != null)
            {
                _scorePresenter.Present(_progress.score);
            }
            if (_goldPresenter != null)
            {
                _goldPresenter.Present(_progress.gold);
            }
            // 설정 화면이 열려 있으면 그 뒤의 일시정지 버튼을 누르지 않도록 일시정지 화면을 숨긴다. 강화 화면도 같은 일시정지라 그동안 숨긴다
            if (_pausePresenter != null)
            {
                _pausePresenter.Present(_progress.isPaused && !isSettingsOpen && !_progress.isUpgradeOpen);
            }
            if (_upgradePresenter != null)
            {
                _upgradePresenter.Present(_progress.isUpgradeOpen && phase == GamePhase.Playing);
            }

            if (!TryFindAlivePlayer(out int playerIndex))
            {
                if (_playerHealthPresenter != null)
                {
                    _playerHealthPresenter.Present(false, 0f, 0f);
                }
                if (_ammoPresenter != null)
                {
                    _ammoPresenter.Present(false, false, WeaponKind.Melee, 0, 0);
                }
                if (_playerManaPresenter != null)
                {
                    _playerManaPresenter.Present(false, 0f, 0f);
                }
                if (_reloadPresenter != null)
                {
                    _reloadPresenter.Present(false, 0, 0);
                }
            }
            else
            {
                PresentPlayerInterface(playerIndex);
            }

            PresentEnemyInterface();
        }

        private bool TryFindAlivePlayer(out int playerIndex)
        {
            PlayerEntity[] entities = _players.entities;
            for (playerIndex = 0; playerIndex < entities.Length; playerIndex++)
            {
                if (entities[playerIndex].isAlive)
                {
                    return true;
                }
            }

            playerIndex = -1;
            return false;
        }

        private void PresentPlayerInterface(int playerIndex)
        {
            ref readonly PlayerEntity entity = ref _players.entities[playerIndex];

            if (_playerHealthPresenter != null)
            {
                _playerHealthPresenter.Present(true, entity.health.current, entity.health.max);
            }

            if (_ammoPresenter != null)
            {
                WeaponEntity[] weapons = _weapons.entities;
                ref readonly PlayerWeaponSlotsComponent weaponSlot = ref entity.weaponSlots;
                int weaponIndex = weaponSlot.currentIndex;
                if (weaponIndex >= 0 && weaponIndex < weaponSlot.weapons.Length)
                {
                    ref readonly WeaponEntity weaponEntity = ref weapons[weaponSlot.weapons[weaponIndex].index];
                    _ammoPresenter.Present(true, true, weaponEntity.weapon.kind, weaponEntity.ammoState.current, weaponEntity.ammoState.reserve);
                }
                else
                {
                    _ammoPresenter.Present(true, false, WeaponKind.Melee, 0, 0);
                }
            }

            if (_playerManaPresenter != null)
            {
                _playerManaPresenter.Present(true, entity.mana.current, entity.mana.max);
            }

            if (_upgradePresenter != null && _progress.isUpgradeOpen)
            {
                PresentUpgradeInterface(in entity.vitals, in entity.upgrade, in entity.upgradeLevels, in entity.weaponSlots, _weapons.entities, _progress.gold);
            }

            if (_reloadPresenter != null)
            {
                _reloadPresenter.Present(entity.actionState.state == PlayerState.Reload, entity.actionState.elapsedTicks, entity.action.reloadTicks);
            }
        }

        // 항목별 레벨, 비용, 지금과 다음 레벨의 수치를 넘긴다. 최대 레벨이 아니고 골드가 비용 이상일 때만 버튼을 누를 수 있다.
        // 다음 레벨 수치는 최대 레벨이어도 한 레벨 위로 계산한다. 그래야 값이 같은지로 이 항목이 강화하지 않는 효과를 가린다
        private void PresentUpgradeInterface(in PlayerVitalsComponent vitals, in PlayerUpgradeComponent upgrade, in PlayerUpgradeLevelsComponent levels, in PlayerWeaponSlotsComponent weaponSlots, WeaponEntity[] weapons, int gold)
        {
            int level = levels.maxHealthLevel;
            PresentUpgradeStat(PlayerUpgradeCommandType.MaxHealth, level, in upgrade, gold,
                PlayerUpgradeUtility.Increase(vitals.startingMaxHealth, upgrade.maxHealthRatioPerLevel, level),
                PlayerUpgradeUtility.Increase(vitals.startingMaxHealth, upgrade.maxHealthRatioPerLevel, level + 1));
            level = levels.maxManaLevel;
            PresentUpgradeStat(PlayerUpgradeCommandType.MaxMana, level, in upgrade, gold,
                PlayerUpgradeUtility.Increase(vitals.startingMaxMana, upgrade.maxManaRatioPerLevel, level),
                PlayerUpgradeUtility.Increase(vitals.startingMaxMana, upgrade.maxManaRatioPerLevel, level + 1));
            level = levels.runManaCostLevel;
            PresentUpgradeStat(PlayerUpgradeCommandType.RunManaCost, level, in upgrade, gold,
                PlayerUpgradeUtility.Decrease(vitals.runManaCostPerTick, upgrade.runManaCostRatioPerLevel, level),
                PlayerUpgradeUtility.Decrease(vitals.runManaCostPerTick, upgrade.runManaCostRatioPerLevel, level + 1));
            level = levels.dodgeManaCostLevel;
            PresentUpgradeStat(PlayerUpgradeCommandType.DodgeManaCost, level, in upgrade, gold,
                PlayerUpgradeUtility.Decrease(vitals.dodgeManaCost, upgrade.dodgeManaCostRatioPerLevel, level),
                PlayerUpgradeUtility.Decrease(vitals.dodgeManaCost, upgrade.dodgeManaCostRatioPerLevel, level + 1));
            level = levels.jumpManaCostLevel;
            PresentUpgradeStat(PlayerUpgradeCommandType.JumpManaCost, level, in upgrade, gold,
                PlayerUpgradeUtility.Decrease(vitals.jumpManaCost, upgrade.jumpManaCostRatioPerLevel, level),
                PlayerUpgradeUtility.Decrease(vitals.jumpManaCost, upgrade.jumpManaCostRatioPerLevel, level + 1));

            EntityIdentifier[] weaponIdentifiers = weaponSlots.weapons;
            for (int weaponIndex = 0; weaponIndex < weaponIdentifiers.Length; weaponIndex++)
            {
                ref readonly WeaponEntity weaponEntity = ref weapons[weaponIdentifiers[weaponIndex].index];
                int weaponLevel = weaponEntity.upgradeLevel.level;
                int cost = PlayerUpgradeUtility.GetCost(in upgrade, weaponLevel);
                WeaponUpgradeEffectComponent effect = GetWeaponUpgradeEffect(in weaponEntity.weapon, in upgrade, weaponLevel);
                _upgradePresenter.PresentWeapon(weaponIndex, weaponLevel, upgrade.maxLevel, cost, CanUpgrade(weaponLevel, cost, in upgrade, gold), in effect);
            }
        }

        private void PresentUpgradeStat(PlayerUpgradeCommandType type, int level, in PlayerUpgradeComponent upgrade, int gold, float value, float nextValue)
        {
            int cost = PlayerUpgradeUtility.GetCost(in upgrade, level);
            _upgradePresenter.PresentStat(type, level, upgrade.maxLevel, cost, CanUpgrade(level, cost, in upgrade, gold), value, nextValue);
        }

        private static WeaponUpgradeEffectComponent GetWeaponUpgradeEffect(in WeaponComponent weapon, in PlayerUpgradeComponent upgrade, int level)
        {
            int nextLevel = level + 1;
            return new WeaponUpgradeEffectComponent
            {
                currentDamage = PlayerUpgradeUtility.GetDamage(in weapon, in upgrade, level),
                nextDamage = PlayerUpgradeUtility.GetDamage(in weapon, in upgrade, nextLevel),
                currentAttackIntervalTicks = PlayerUpgradeUtility.GetAttackIntervalTicks(in weapon, in upgrade, level),
                nextAttackIntervalTicks = PlayerUpgradeUtility.GetAttackIntervalTicks(in weapon, in upgrade, nextLevel),
                currentMaxAmmo = PlayerUpgradeUtility.GetMaxAmmo(in weapon, level),
                nextMaxAmmo = PlayerUpgradeUtility.GetMaxAmmo(in weapon, nextLevel),
                currentExplosionRadius = PlayerUpgradeUtility.GetExplosionRadius(in weapon, level),
                nextExplosionRadius = PlayerUpgradeUtility.GetExplosionRadius(in weapon, nextLevel),
                currentMeleeRadius = weapon.meleeShape.radius * PlayerUpgradeUtility.GetMeleeRangeScale(in weapon, level),
                nextMeleeRadius = weapon.meleeShape.radius * PlayerUpgradeUtility.GetMeleeRangeScale(in weapon, nextLevel)
            };
        }

        private static bool CanUpgrade(int level, int cost, in PlayerUpgradeComponent upgrade, int gold)
        {
            return level < upgrade.maxLevel && gold >= cost;
        }

        private void PresentEnemyInterface()
        {
            if (_bossHealthPresenter != null)
            {
                EnemyEntity[] entities = _enemies.entities;
                bool hasOverlayEnemy = false;
                for (int index = 0; index < entities.Length; index++)
                {
                    ref readonly EnemyEntity entity = ref entities[index];
                    if (entity.isAlive && _enemyShowsOverlayHealthBar[entity.kind.kind])
                    {
                        _bossHealthPresenter.Present(true, entity.health.current, entity.health.max);
                        hasOverlayEnemy = true;
                        break;
                    }
                }
                if (!hasOverlayEnemy)
                {
                    _bossHealthPresenter.Present(false, 0f, 0f);
                }
            }
        }
    }
}
