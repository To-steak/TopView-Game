using TopView.Components;
using TopView.Entities;
using TopView.UserInterface.Ammo;
using TopView.UserInterface.BossHealth;
using TopView.UserInterface.GameOver;
using TopView.UserInterface.Gold;
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
        private GoldPresenter _goldPresenter;
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

            _goldPresenter = FindAnyObjectByType<GoldPresenter>();
            if (_goldPresenter == null)
            {
                Debug.LogError("Runner >> 씬에 GoldPresenter가 없다.");
            }
            else
            {
                _goldPresenter.Initialize();
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
            if (_goldPresenter != null)
            {
                _goldPresenter.Present(_world.progress.gold);
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
            ComponentMask[] masks = _world.entities.masks;
            for (playerIndex = 0; playerIndex < masks.Length; playerIndex++)
            {
                if ((masks[playerIndex] & ComponentMask.PlayerArchetype) == ComponentMask.PlayerArchetype)
                {
                    return true;
                }
            }

            playerIndex = -1;
            return false;
        }

        private void PresentPlayerInterface(int playerIndex)
        {
            EntityStorage entities = _world.entities;

            if (_playerHealthPresenter != null)
            {
                _playerHealthPresenter.Present(true, entities.healths[playerIndex].current, entities.healths[playerIndex].max);
            }

            if (_ammoPresenter != null)
            {
                WeaponComponent[] weapons = entities.weaponLists[playerIndex].weapons;
                ref readonly PlayerWeaponSlotsComponent weaponSlot = ref entities.weaponSlots[playerIndex];
                int weaponIndex = weaponSlot.currentIndex;
                if (weaponIndex >= 0 && weaponIndex < weapons.Length)
                {
                    _ammoPresenter.Present(true, true, weapons[weaponIndex].kind, weaponSlot.currentAmmo[weaponIndex], weaponSlot.reserveAmmo[weaponIndex]);
                }
                else
                {
                    _ammoPresenter.Present(true, false, WeaponKind.Melee, 0, 0);
                }
            }

            if (_playerManaPresenter != null)
            {
                _playerManaPresenter.Present(true, entities.manas[playerIndex].current, entities.playerVitals[playerIndex].maxMana);
            }
        }

        private void PresentEnemyInterface()
        {
            if (_bossHealthPresenter != null)
            {
                EntityStorage entities = _world.entities;
                bool hasBoss = false;
                for (int index = 0; index < entities.masks.Length; index++)
                {
                    if ((entities.masks[index] & ComponentMask.BossArchetype) == ComponentMask.BossArchetype)
                    {
                        _bossHealthPresenter.Present(true, entities.healths[index].current, entities.healths[index].max);
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
