using TopView.Components;
using TopView.Entities;
using TopView.UserInterface.Ammo;
using TopView.UserInterface.BossHealth;
using TopView.UserInterface.GameOver;
using TopView.UserInterface.PlayerHealth;
using TopView.UserInterface.PlayerMana;
using TopView.UserInterface.RemainingTime;
using TopView.UserInterface.Score;
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
        private PlayerManaPresenter _playerManaPresenter;
        private BossHealthPresenter _bossHealthPresenter;

        private void BindPresenters()
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

            _playerManaPresenter = FindAnyObjectByType<PlayerManaPresenter>();
            if (_playerManaPresenter == null)
            {
                Debug.LogError("Runner >> 씬에 PlayerManaPresenter가 없다.");
            }
            else
            {
                _playerManaPresenter.Initialize();
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
                if (_playerManaPresenter != null)
                {
                    _playerManaPresenter.Present(false, 0f, 0f);
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
            for (playerIndex = 0; playerIndex < _world.players.entities.Length; playerIndex++)
            {
                if (_world.players.entities[playerIndex].isAlive)
                {
                    return true;
                }
            }

            playerIndex = -1;
            return false;
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

            if (_playerManaPresenter != null)
            {
                _playerManaPresenter.Present(true, player.mana, player.vitals.maxMana);
            }
        }

        private void PresentEnemyInterface()
        {
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
