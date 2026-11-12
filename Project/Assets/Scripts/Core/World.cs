using TopView.Components;
using TopView.Definitions;
using TopView.Presentation.ViewContexts;
using TopView.Presentation.ViewSystems;
using TopView.Systems;
using UnityEngine;

namespace TopView.Core
{
    // 판이 있는 씬에 하나 배치해 그 판의 틱 순서와 호출 순서를 관리한다. 씬과 함께 생기고 사라진다.
    // 요소별 상태와 처리는 World.<요소>.cs에, 한 판의 데이터는 World.Data.cs에 나눠 두고, 이 파일에는 호출 순서만 둔다.
    public partial class World : MonoBehaviour
    {
        // 한 판의 규칙과 저장소 용량
        [SerializeField] private GameSettingsDefinition _gameSettingsDefinition;

        private ViewContext _viewContext;

        private void Awake()
        {
            _playerActions = new PlayerActions();
            _playerActions.Global.Enable();
            _playerActions.UserInterface.Enable();
        }

        // 씬의 다른 오브젝트가 Awake를 마친 뒤에 판을 준비한다. Audio Mixer는 Awake에서 값을 무시할 수 있어 볼륨도 여기서 반영한다
        private void Start()
        {
            PresentSavedAudioVolume();
            StartRound();
        }

        private void OnDestroy()
        {
            // 일시정지 중에 씬이 사라져도 소리와 DOTween의 일시정지가 다음 씬에 남지 않게 한다
            ClearRound();
            _playerActions.Global.Disable();
            _playerActions.UserInterface.Disable();
            _playerActions.Dispose();
        }

        private void FixedUpdate()
        {
            if (!_isRoundReady)
            {
                return;
            }

            // 이번 틱이 돌지 말지를 정하므로 맨 앞에 둔다
            PauseSystem.Tick(ref _progress, _commandBuffer.pauseCommands);
            // 탈출 버튼은 일시정지 화면에 있어 isPlaying과 상관없이 돈다. 같은 버튼이 넣은 Resume을 PauseSystem이 먼저 처리한다
            PlayerUnstuckSystem.Tick(_players, in _progress, in _rule, _commandBuffer.playerUnstuckCommands);
            // 강화 화면은 일시정지 중에 열리므로 isPlaying과 상관없이 돈다. 연 틱의 명령도 처리하도록 PauseSystem 뒤에 둔다
            PlayerUpgradeSystem.Tick(_players, _weapons, ref _progress, _commandBuffer.playerUpgradeCommands);
            // 강화 화면에서 바로 쓸 수 있도록 일시정지 중에도 돈다
            DebugCommandSystem.Tick(ref _progress, _commandBuffer.debugCommands);
            bool isPlaying = _progress.phase == GamePhase.Playing && !_progress.isPaused;
            DamageSystem.Tick(ref _progress, _players, _enemies, _playerDamageRequests, _enemyDamageRequests, _enemyDamagedRecords);
            if (isPlaying)
            {
                TransformSnapshotSystem.Tick(_players, _enemies, _projectiles);
                PlayerDeathSystem.Tick(_players, _playerStateChangedRecords);
                MeleeOwnerDeathSystem.Tick(_melees, _players, _meleeDespawnRequests);
                MeleeDespawnSystem.Tick(_melees, _meleeDespawnRequests);
                WeaponOwnerDeathSystem.Tick(_weapons, _players, _weaponDespawnRequests);
                WeaponDespawnSystem.Tick(_weapons, _weaponDespawnRequests);
                PlayerStateTimerSystem.Tick(_players);
                // 누름 입력으로 정해진 상태를 뒤의 셋이 함께 읽는다. 이동 입력과 공격을 반영한 뒤에 상태를 넘기므로 이 순서를 지킨다
                PlayerPressSystem.Tick(_players, _weapons, in _commandBuffer.playerCommand, _playerStateChangeRequests);
                PlayerStateChangeSystem.Tick(_players, _weapons, _playerStateChangeRequests, _playerStateChangedRecords);
                PlayerMoveInputSystem.Tick(_players, in _commandBuffer.playerCommand);
                PlayerAttackSystem.Tick(_players, _weapons, _projectileSpawnRequests, _meleeSpawnRequests, _playerAttackCommittedRecords);
                PlayerStateSystem.Tick(_players, _weapons, in _commandBuffer.playerCommand, _playerStateChangeRequests, _gameOverRequests);
                PlayerStateChangeSystem.Tick(_players, _weapons, _playerStateChangeRequests, _playerStateChangedRecords);
                // 막기는 이동이 끝난 위치를 읽고, 조준은 막힌 뒤의 위치를 읽으므로 이 순서를 지킨다
                PlayerMovementSystem.Tick(_players);
                PlayerBlockSystem.Tick(_players, _enemies);
                PlayerAimSystem.Tick(_players, in _commandBuffer.playerCommand);
                // 미리보기는 이번 틱의 충전량과 이동·조준이 끝난 위치와 회전을 읽으므로 조준 뒤에 둔다
                PlayerThrowPreviewSystem.Tick(_players, _weapons);
                PlayerStaminaSystem.Tick(_players);
                // 획득은 이동이 끝난 위치를 읽고, 이번 틱 피해가 반영된 체력을 보므로 이 자리에 둔다
                LootPickupSystem.Tick(_loots, _players, _weapons, ref _progress, _lootRule.pickupRadius, _lootDespawnRequests, _lootPickedUpRecords);
                LootDespawnSystem.Tick(_loots, _lootDespawnRequests);
                LootLifetimeSystem.Tick(_loots, _lootDespawnRequests);
                LootDespawnSystem.Tick(_loots, _lootDespawnRequests);
                EnemyDeathSystem.Tick(_enemies, _enemyStateChangedRecords);
                EnemyStateTimerSystem.Tick(_enemies);
                PlayerLocationSystem.Tick(_players, ref _playerLocation);
                // 공격 상태는 상태 시간이 0이면 건너뛰므로, 공격이 끝나 Idle이 된 적과 막 공격에 들어온 적 모두 다음 틱부터 움직이도록 이 순서를 지킨다
                EnemyBehaviorSystem.Tick(_enemies, in _playerLocation, _random, _enemyStateChangeRequests);
                EnemyStateChangeSystem.Tick(_enemies, _enemyStateChangeRequests, _enemyStateChangedRecords);
                EnemyAttackStateSystem.Tick(_enemies, in _playerLocation, _enemyStateChangeRequests, _projectileSpawnRequests, _meleeSpawnRequests, _enemyAttackCommittedRecords);
                EnemyStateChangeSystem.Tick(_enemies, _enemyStateChangeRequests, _enemyStateChangedRecords);
                EnemyDespawnSystem.Tick(_enemies, _gameOverRequests, _random, _lootSpawnRequests);
                EnemyNavigationSystem.Tick(_enemies, _navigationPath, _rule.navMeshSampleDistance);
                ProjectileSystem.Tick(_projectiles, _projectileDespawnRequests);
                ProjectileDespawnSystem.Tick(_projectiles, _projectileDespawnRequests);
                HitTargetSystem.Tick(_players, _enemies, _playerHitTargets, ref _playerHitTargetCount, _enemyHitTargets, ref _enemyHitTargetCount);
                ProjectileHitSystem.Tick(_projectiles, _playerHitTargets, _playerHitTargetCount, _enemyHitTargets, _enemyHitTargetCount, _playerDamageRequests, _enemyDamageRequests, _projectileHitRecords, _projectileDespawnRequests);
                ProjectileDespawnSystem.Tick(_projectiles, _projectileDespawnRequests);
                MeleeHitSystem.Tick(_melees, _playerHitTargets, _playerHitTargetCount, _enemyHitTargets, _enemyHitTargetCount, _playerDamageRequests, _enemyDamageRequests, _meleeDespawnRequests);
                MeleeDespawnSystem.Tick(_melees, _meleeDespawnRequests);
            }

            GameRuleSystem.Tick(ref _progress, _gameOverRequests);
            if (isPlaying)
            {
                PlayerDetectorSystem.Tick(_playerDetectors, _playerDetectorStates, in _playerLocation, _enemySpawnerActivationRequests);
                // 레벨이 오른 틱에 생기는 적도 새 레벨로 나오도록 스포너보다 먼저 둔다
                EnemyLevelSystem.Tick(_rule.enemyLevelUpIntervalTicks, ref _progress);
                EnemySpawnerSystem.Tick(_progress.enemyLevel, _enemySpawners, _enemySpawnerStates, _enemySpawnTemplates, _enemySpawnerActivationRequests, _enemySpawnRequests);
                PlayerSpawnSystem.Tick(_players, _playerSpawnRequests, _weaponSpawnRequests);
                WeaponSpawnSystem.Tick(_weapons, _players, _weaponSpawnRequests);
                EnemySpawnSystem.Tick(_enemies, _enemySpawnRequests, _rule.navMeshSampleDistance);
                ProjectileSpawnSystem.Tick(_projectiles, _projectileSpawnRequests);
                MeleeSpawnSystem.Tick(_melees, _meleeSpawnRequests);
                LootSpawnSystem.Tick(_loots, _lootSpawnRequests, _lootRule.lifetimeTicks);
            }

            CommandSystem.ClearPlayerPresses(ref _commandBuffer.playerCommand);
        }

        private void Update()
        {
            if (!_isRoundReady)
            {
                return;
            }

            ReadEscapeInput(_commandBuffer, in _progress);
            ReadUpgradeInput(_commandBuffer, in _progress);
            if (_progress.isPaused)
            {
                ClearHeldPlayerInput(ref _commandBuffer.playerCommand);
            }
            else
            {
                ReadPlayerInput(ref _commandBuffer.playerCommand);
            }
            HandleSettingsCommands(_commandBuffer.settingsCommands, in _progress);
            // 씬을 바꾸면 판을 비우므로 마지막에 둔다
            HandleSceneCommands(_commandBuffer.sceneCommands);
        }

        private void LateUpdate()
        {
            if (!_isRoundReady)
            {
                return;
            }

            bool isPaused = _progress.isPaused;
            // 이번 프레임에 일시정지가 걸리거나 풀리면 소리와 DOTween에 먼저 반영한다
            PauseViewSystem.Present(isPaused, _viewContext.pause);
            CursorViewSystem.Present(isPaused, _gameSettingsDefinition.crosshairCursor, _viewContext.cursor);
            PlayerViewSystem.Present(_players.entities, _weapons.entities, _playerStateChangedRecords, _playerAttackCommittedRecords, _playerViewPrefab, _playerWeaponModelPrefabs, _playerRangedLayerBlendInTicks, isPaused, _viewContext.player);
            PlayerThrowPreviewViewSystem.Present(_players.entities, _viewContext.player.view, _playerThrowPreviewCircleHeightOffset);
            EnemyViewSystem.Present(_enemies.entities, _enemyStateChangedRecords, _enemyAttackCommittedRecords, _enemyDamagedRecords, _enemyViewPrefabs, _enemyAttackAnimations, _enemyAttackClips, isPaused, _viewContext.enemy);
            EnemyHealthGaugeViewSystem.Present(_enemies.entities, _enemyHealthGaugePrefabs, _viewContext.camera.mainCamera, _viewContext.enemy);
            WorldInformationViewSystem.Present(_viewContext.camera.mainCamera, _viewContext.worldInformation);
            ProjectileViewSystem.Present(_projectiles.entities, _projectilePrefabs, isPaused, _viewContext.projectile);
            LootViewSystem.Present(_loots.entities, _lootPrefabs, isPaused, _viewContext.loot);
            HitEffectViewSystem.Present(_projectileHitRecords, _playerAttackCommittedRecords, _hitEffectPrefabs, isPaused, _viewContext.hitEffect);
            DamageTextViewSystem.Present(_enemyDamagedRecords, _viewContext.enemy.views, _viewContext.enemy.viewGenerations, _worldTextViewPrefab, _viewContext.camera.mainCamera, _viewContext.worldText.pendingTexts);
            LootTextViewSystem.Present(_lootPickedUpRecords, _worldTextViewPrefab, _viewContext.worldText.pendingTexts);
            LootSoundViewSystem.Present(_lootPickedUpRecords, _lootPrefabs, _viewContext.player.view != null ? _viewContext.player.view.soundEffectSource : null);
            WorldTextViewSystem.Present(_worldTextViewPrefab, _viewContext.camera.mainCamera, _viewContext.worldText);
            CameraViewSystem.Present(_viewContext.camera, _viewContext.player.view, _playerCameraPriority, _commandBuffer.playerCommand.cameraYawInput, _playerCameraYawDegreesPerSecond);
            if (_viewContext.music != null)
            {
                MusicViewSystem.Present(_enemies.entities, _enemyShowsOverlayHealthBar, _viewContext.music);
            }

            PresentUserInterface();
            if (_viewContext.userInterfaceSound != null)
            {
                UserInterfaceSoundViewSystem.Present(_userInterfaceSoundRequests, _viewContext.userInterfaceSound);
            }
            _userInterfaceSoundRequests.Clear();

            _playerStateChangedRecords.Clear();
            _playerAttackCommittedRecords.Clear();
            _enemyStateChangedRecords.Clear();
            _enemyAttackCommittedRecords.Clear();
            _projectileHitRecords.Clear();
            _enemyDamagedRecords.Clear();
            _lootPickedUpRecords.Clear();
        }
    }
}
