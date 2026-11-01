using TopView.Components;
using TopView.Definitions;
using TopView.Entities;
using TopView.Presentation.Enemy.Views;
using TopView.UserInterface.Ammo;
using TopView.UserInterface.BossHealth;
using TopView.UserInterface.GameOver;
using TopView.UserInterface.PlayerHealth;
using TopView.UserInterface.RemainingTime;
using TopView.UserInterface.Score;
using TopView.UserInterface.WorldGauge;
using UnityEngine;

namespace TopView.Core
{
    // UI: Presenter를 찾아 연결하고, 매 프레임 필요한 데이터만 넘긴다
    public partial class Runner
    {
        private GameOverPresenter _gameOverPresenter;
        private RemainingTimePresenter _remainingTimePresenter;
        private ScorePresenter _scorePresenter;
        private PlayerHealthPresenter _playerHealthPresenter;
        private AmmoPresenter _ammoPresenter;
        private PlayerManaGaugePresenter _playerManaGaugePresenter;
        private BossHealthPresenter _bossHealthPresenter;
        private EnemyHealthGaugePresenter _enemyHealthGaugePresenter;

        private void BindPresenters(PlayerDefinition playerDefinition, Vector3[] gaugeOffsets, Color[] gaugeColors)
        {
            _gameOverPresenter = FindAnyObjectByType<GameOverPresenter>();
            if (_gameOverPresenter == null)
            {
                Debug.LogError("Runner >> 씬에 GameOverPresenter가 없다.");
            }
            else
            {
                _gameOverPresenter.Initialize(_world.commandBuffer);
            }

            _remainingTimePresenter = FindAnyObjectByType<RemainingTimePresenter>();
            if (_remainingTimePresenter == null)
            {
                Debug.LogError("Runner >> 씬에 RemainingTimePresenter가 없다.");
            }
            else
            {
                _remainingTimePresenter.Initialize();
            }

            _scorePresenter = FindAnyObjectByType<ScorePresenter>();
            if (_scorePresenter == null)
            {
                Debug.LogError("Runner >> 씬에 ScorePresenter가 없다.");
            }
            else
            {
                _scorePresenter.Initialize();
            }

            _playerHealthPresenter = FindAnyObjectByType<PlayerHealthPresenter>();
            if (_playerHealthPresenter == null)
            {
                Debug.LogError("Runner >> 씬에 PlayerHealthPresenter가 없다.");
            }
            else
            {
                _playerHealthPresenter.Initialize();
            }

            _ammoPresenter = FindAnyObjectByType<AmmoPresenter>();
            if (_ammoPresenter == null)
            {
                Debug.LogError("Runner >> 씬에 AmmoPresenter가 없다.");
            }
            else
            {
                _ammoPresenter.Initialize();
            }

            _playerManaGaugePresenter = FindAnyObjectByType<PlayerManaGaugePresenter>();
            if (_playerManaGaugePresenter == null)
            {
                Debug.LogError("Runner >> 씬에 PlayerManaGaugePresenter가 없다.");
            }
            else
            {
                _playerManaGaugePresenter.Initialize(playerDefinition.manaGaugeOffset);
            }

            _bossHealthPresenter = FindAnyObjectByType<BossHealthPresenter>();
            if (_bossHealthPresenter == null)
            {
                Debug.LogError("Runner >> 씬에 BossHealthPresenter가 없다.");
            }
            else
            {
                _bossHealthPresenter.Initialize();
            }

            _enemyHealthGaugePresenter = FindAnyObjectByType<EnemyHealthGaugePresenter>();
            if (_enemyHealthGaugePresenter == null)
            {
                Debug.LogError("Runner >> 씬에 EnemyHealthGaugePresenter가 없다.");
            }
            else
            {
                _enemyHealthGaugePresenter.Initialize(_world.enemies.entities.Length, gaugeOffsets, gaugeColors);
            }
        }

        private void PresentUserInterface()
        {
            GamePhase phase = _world.progress.phase;
            if (_gameOverPresenter != null)
            {
                _gameOverPresenter.Present(phase);
            }
            if (_remainingTimePresenter != null)
            {
                _remainingTimePresenter.Present(_world.progress.remainingTicks);
            }
            if (_scorePresenter != null)
            {
                _scorePresenter.Present(_world.progress.score);
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
                if (_playerManaGaugePresenter != null)
                {
                    _playerManaGaugePresenter.Present(null, 0f, 0f);
                }
            }
            else
            {
                PresentPlayerInterface(playerIndex);
            }

            PresentEnemyInterface();
        }

        private void PresentPlayerInterface(int playerIndex)
        {
            ref readonly PlayerEntity player = ref _world.players.entities[playerIndex];

            if (_playerHealthPresenter != null)
            {
                _playerHealthPresenter.Present(true, player.health, player.vitals.maxHealth);
            }

            if (_ammoPresenter != null)
            {
                int weaponIndex = player.currentWeaponIndex;
                if (weaponIndex >= 0 && weaponIndex < player.weapons.Length)
                {
                    ref readonly WeaponComponent weapon = ref player.weapons[weaponIndex];
                    _ammoPresenter.Present(true, true, weapon.kind, weapon.currentAmmo, weapon.reserveAmmo);
                }
                else
                {
                    _ammoPresenter.Present(true, false, WeaponKind.Melee, 0, 0);
                }
            }

            if (_playerManaGaugePresenter != null)
            {
                Transform target = _viewContext.playerView != null ? _viewContext.playerView.transform : null;
                _playerManaGaugePresenter.Present(target, player.mana, player.vitals.maxMana);
            }
        }

        private void PresentEnemyInterface()
        {
            if (_enemyHealthGaugePresenter != null)
            {
                EnemyEntity[] enemies = _world.enemies.entities;
                for (int enemyIndex = 0; enemyIndex < enemies.Length; enemyIndex++)
                {
                    ref readonly EnemyEntity enemy = ref enemies[enemyIndex];
                    EnemyView view = _viewContext.enemyViews[enemyIndex];
                    Transform target = enemy.isAlive && view != null ? view.transform : null;
                    _enemyHealthGaugePresenter.Present(enemyIndex, enemy.kind, target, enemy.health, enemy.maxHealth);
                }
            }

            if (_bossHealthPresenter != null)
            {
                BossEntity[] bosses = _world.bosses.entities;
                bool hasBoss = false;
                for (int bossIndex = 0; bossIndex < bosses.Length; bossIndex++)
                {
                    ref readonly BossEntity boss = ref bosses[bossIndex];
                    if (boss.isAlive)
                    {
                        _bossHealthPresenter.Present(true, boss.health, boss.maxHealth);
                        hasBoss = true;
                        break;
                    }
                }
                if (!hasBoss)
                {
                    _bossHealthPresenter.Present(false, 0f, 0f);
                }
            }
        }
    }
}
