using System.Collections.Generic;
using TopView.Components;
using TopView.Definitions;
using TopView.Entities;
using TopView.Presentation.ViewContexts;
using TopView.UserInterface.Presenters;
using UnityEngine;

namespace TopView.Core
{
    // UI: Presenter를 찾아 연결하고, 매 프레임 필요한 데이터만 넘긴다
    public partial class Runner
    {
        // 버튼 소리
        [SerializeField] private UserInterfaceSoundDefinition _userInterfaceSoundDefinition;

        // Presenter가 이번 프레임에 넣은 버튼 소리 요청. LateUpdate 끝에 비운다
        private readonly List<UserInterfaceSoundKind> _userInterfaceSoundRequests = new List<UserInterfaceSoundKind>();
        private GameOverPresenter _gameOverPresenter;
        private RemainingTimePresenter _remainingTimePresenter;
        private ScorePresenter _scorePresenter;
        private GoldPresenter _goldPresenter;
        private PlayerHealthPresenter _playerHealthPresenter;
        private AmmoPresenter _ammoPresenter;
        private PlayerManaPresenter _playerManaPresenter;
        private ReloadPresenter _reloadPresenter;
        private BossHealthPresenter _bossHealthPresenter;
        private PausePresenter _pausePresenter;
        private SettingsPresenter _settingsPresenter;

        private const string USER_INTERFACE_SOUND_OBJECT_NAME = "User Interface Sound";

        private void BindPresenters()
        {
            _gameOverPresenter = FindAnyObjectByType<GameOverPresenter>();
            if (_gameOverPresenter == null)
            {
                Debug.LogError("Runner >> 씬에 GameOverPresenter가 없다.");
            }
            else
            {
                _gameOverPresenter.Initialize(_world.commandBuffer, _userInterfaceSoundRequests);
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

            _reloadPresenter = FindAnyObjectByType<ReloadPresenter>();
            if (_reloadPresenter == null)
            {
                Debug.LogError("Runner >> 씬에 ReloadPresenter가 없다.");
            }
            else
            {
                _reloadPresenter.Initialize();
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

            _pausePresenter = FindAnyObjectByType<PausePresenter>();
            if (_pausePresenter == null)
            {
                Debug.LogError("Runner >> 씬에 PausePresenter가 없다.");
            }
            else
            {
                _pausePresenter.Initialize(_world.commandBuffer, _userInterfaceSoundRequests);
            }

            _settingsPresenter = FindAnyObjectByType<SettingsPresenter>();
            if (_settingsPresenter == null)
            {
                Debug.LogError("Runner >> 씬에 SettingsPresenter가 없다.");
            }
            else
            {
                _settingsPresenter.Initialize(_world.commandBuffer.settingsCommands, _userInterfaceSoundRequests);
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
            // 설정 화면이 열려 있으면 그 뒤의 일시정지 버튼을 누르지 않도록 일시정지 화면을 숨긴다
            if (_pausePresenter != null)
            {
                _pausePresenter.Present(_world.progress.isPaused && !_isSettingsOpen);
            }
            if (_settingsPresenter != null)
            {
                _settingsPresenter.Present(_isSettingsOpen, in _volume);
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

        // 판이 있는 씬에 UI 소리 오브젝트를 만든다. 일시정지 중에도 버튼 소리가 들리도록 ignoreListenerPause를 켠다.
        // User Interface Sound Definition이 없으면 null을 돌려준다. MenuFlow에 같은 코드가 있다. Runner를 나눌 때 함께 정리한다
        private UserInterfaceSoundViewContext CreateUserInterfaceSoundViewContext()
        {
            UserInterfaceSoundDefinition definition = _userInterfaceSoundDefinition;
            if (definition == null)
            {
                Debug.LogError("Runner >> User Interface Sound Definition이 연결되어 있지 않아 버튼 소리가 나지 않는다.");
                return null;
            }

            if (definition.outputGroup == null)
            {
                Debug.LogWarning("Runner >> User Interface Sound Definition에 Output Group이 연결되어 있지 않아 버튼 소리가 Audio Mixer를 거치지 않는다.");
            }

            GameObject soundObject = new GameObject(USER_INTERFACE_SOUND_OBJECT_NAME);
            AudioSource source = soundObject.AddComponent<AudioSource>();
            source.outputAudioMixerGroup = definition.outputGroup;
            source.playOnAwake = false;
            source.spatialBlend = 0f;
            source.ignoreListenerPause = true;
            return new UserInterfaceSoundViewContext
            {
                source = source,
                clickClips = definition.clickClips,
                closeClips = definition.closeClips,
                volume = definition.volume
            };
        }

        private bool TryFindAlivePlayer(out int playerIndex)
        {
            PlayerEntity[] entities = _world.players.entities;
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
            ref readonly PlayerEntity entity = ref _world.players.entities[playerIndex];

            if (_playerHealthPresenter != null)
            {
                _playerHealthPresenter.Present(true, entity.health.current, entity.health.max);
            }

            if (_ammoPresenter != null)
            {
                WeaponComponent[] weapons = entity.weaponList.weapons;
                ref readonly PlayerWeaponSlotsComponent weaponSlot = ref entity.weaponSlots;
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
                _playerManaPresenter.Present(true, entity.mana.current, entity.vitals.maxMana);
            }

            if (_reloadPresenter != null)
            {
                _reloadPresenter.Present(entity.actionState.state == PlayerState.Reload, entity.actionState.elapsedTicks, entity.action.reloadTicks);
            }
        }

        private void PresentEnemyInterface()
        {
            if (_bossHealthPresenter != null)
            {
                EnemyEntity[] entities = _world.enemies.entities;
                bool hasOverlayEnemy = false;
                for (int index = 0; index < entities.Length; index++)
                {
                    ref readonly EnemyEntity entity = ref entities[index];
                    if (entity.isAlive && entity.behavior.showsOverlayHealthBar)
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
