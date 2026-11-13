using System.Collections.Generic;
using TopView.Commands;
using TopView.Components;
using TopView.Definitions;
using TopView.Presentation.ViewContexts;
using TopView.Presentation.ViewSystems;
using TopView.Systems;
using UnityEngine;

namespace TopView.Core
{
    // 판이 있는 씬에 하나 배치해 그 판의 틱 순서와 호출 순서를 관리한다. 씬과 함께 생기고 사라진다.
    // Unity의 실행 함수는 App이 받고, App이 Tick, ReadInput, Present를 부른다.
    // 요소별 상태와 처리는 World.<요소>.cs에, 한 판의 데이터는 World.Data.cs에 나눠 두고, 이 파일에는 호출 순서만 둔다.
    public partial class World : MonoBehaviour
    {
        // 설정 화면을 열 수 있는지. 설정 버튼은 일시정지 화면에만 있어 일시정지 중에만 연다. App이 설정 명령을 소비할 때 읽는다
        public bool CanOpenSettings => _isRoundReady && _progress.isPaused;

        // 한 판의 규칙과 저장소 용량
        [SerializeField] private GameSettingsDefinition _gameSettingsDefinition;
        // 이 씬의 배경음악 곡과 재생 값. App에 등록할 때 넘긴다
        [SerializeField] private MusicDefinition _musicDefinition;

        private ViewContext _viewContext;

        /// <summary>
        /// App의 명령 목록을 받아 판을 준비한다. App이 World를 등록할 때 부른다.
        /// </summary>
        /// <param name="sceneCommands">씬과 앱 전환 명령 목록. App이 소비한다</param>
        /// <param name="settingsCommands">설정 명령 목록. App이 소비한다</param>
        /// <param name="userInterfaceSoundRequests">UI 소리 요청 목록. App이 소비한다</param>
        public void Initialize(List<SceneCommand> sceneCommands, List<SettingsCommand> settingsCommands, List<UserInterfaceSoundKind> userInterfaceSoundRequests)
        {
            _sceneCommands = sceneCommands;
            _settingsCommands = settingsCommands;
            _userInterfaceSoundRequests = userInterfaceSoundRequests;
            StartRound();
        }

        /// <summary>
        /// 한 틱의 System을 정해진 순서로 부른다. App이 FixedUpdate에서 부른다.
        /// </summary>
        public void Tick()
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
            // 일시정지와 판 종료 중에도 직전 값을 지금 값으로 맞춰야 View가 두 값 사이를 계속 보간하며 떨지 않는다
            TransformSnapshotSystem.Tick(_players, _entities);
            DamageSystem.Tick(ref _progress, _players, _entities, _playerDamageRequests, _enemyDamageRequests, _enemyDamagedRecords);
            if (isPlaying)
            {
                PlayerDeathSystem.Tick(_players, _playerStateChangedRecords);
                MeleeOwnerDeathSystem.Tick(_entities, _players, _meleeDespawnRequests);
                MeleeDespawnSystem.Tick(_entities, _meleeDespawnRequests);
                WeaponOwnerDeathSystem.Tick(_weapons, _players, _weaponDespawnRequests);
                WeaponDespawnSystem.Tick(_weapons, _weaponDespawnRequests);
                PlayerStateTimerSystem.Tick(_players);
                // 누름 입력으로 정해진 상태를 뒤의 셋이 함께 읽는다. 이동 입력과 공격을 반영한 뒤에 상태를 넘기므로 이 순서를 지킨다
                PlayerPressSystem.Tick(_players, _weapons, in _commandBuffer.playerCommand, _playerStateChangeRequests);
                PlayerStateChangeSystem.Tick(_players, _weapons, _playerStateChangeRequests, _playerStateChangedRecords);
                PlayerMoveInputSystem.Tick(_players, in _commandBuffer.playerCommand);
                PlayerAttackSystem.Tick(_players, _weapons, _straightProjectileSpawnRequests, _arcProjectileSpawnRequests, _meleeSpawnRequests, _playerAttackCommittedRecords);
                PlayerStateSystem.Tick(_players, _weapons, in _commandBuffer.playerCommand, _playerStateChangeRequests, _gameOverRequests);
                PlayerStateChangeSystem.Tick(_players, _weapons, _playerStateChangeRequests, _playerStateChangedRecords);
                // 막기는 이동이 끝난 위치를 읽고, 조준은 막힌 뒤의 위치를 읽으므로 이 순서를 지킨다
                PlayerMovementSystem.Tick(_players);
                PlayerBlockSystem.Tick(_players, _entities);
                PlayerAimSystem.Tick(_players, in _commandBuffer.playerCommand);
                // 미리보기는 이번 틱의 충전량과 이동·조준이 끝난 위치와 회전을 읽으므로 조준 뒤에 둔다
                PlayerThrowPreviewSystem.Tick(_players, _weapons);
                PlayerStaminaSystem.Tick(_players);
                // 획득은 이동이 끝난 위치를 읽고, 이번 틱 피해가 반영된 체력을 보므로 이 자리에 둔다
                LootPickupSystem.Tick(_entities, _players, _weapons, ref _progress, _lootRule.pickupRadius, _lootDespawnRequests, _lootPickedUpRecords);
                LootDespawnSystem.Tick(_entities, _lootDespawnRequests);
                LootLifetimeSystem.Tick(_entities, _lootDespawnRequests);
                LootDespawnSystem.Tick(_entities, _lootDespawnRequests);
                EnemyDeathSystem.Tick(_entities, _enemyStateChangedRecords);
                EnemyStateTimerSystem.Tick(_entities);
                PlayerLocationSystem.Tick(_players, ref _playerLocation);
                // 공격 상태는 상태 시간이 0이면 건너뛰므로, 공격이 끝나 Idle이 된 적과 막 공격에 들어온 적 모두 다음 틱부터 움직이도록 이 순서를 지킨다
                EnemyBehaviorSystem.Tick(_entities, in _playerLocation, _random, _enemyStateChangeRequests);
                EnemyStateChangeSystem.Tick(_entities, _enemyStateChangeRequests, _enemyStateChangedRecords);
                EnemyAttackStateSystem.Tick(_entities, in _playerLocation, _enemyStateChangeRequests, _straightProjectileSpawnRequests, _meleeSpawnRequests, _enemyAttackCommittedRecords);
                EnemyStateChangeSystem.Tick(_entities, _enemyStateChangeRequests, _enemyStateChangedRecords);
                EnemyDespawnSystem.Tick(_entities, _gameOverRequests, _random, _lootSpawnRequests);
                EnemyNavigationSystem.Tick(_entities, _navigationPath, _rule.navMeshSampleDistance);
                ProjectileSystem.Tick(_entities, _projectileDespawnRequests);
                ProjectileDespawnSystem.Tick(_entities, _projectileDespawnRequests);
                HitTargetSystem.Tick(_players, _entities, _playerHitTargets, ref _playerHitTargetCount, _enemyHitTargets, ref _enemyHitTargetCount);
                ProjectileHitSystem.Tick(_entities, _playerHitTargets, _playerHitTargetCount, _enemyHitTargets, _enemyHitTargetCount, _playerDamageRequests, _enemyDamageRequests, _projectileHitRecords, _projectileDespawnRequests);
                ProjectileDespawnSystem.Tick(_entities, _projectileDespawnRequests);
                MeleeHitSystem.Tick(_entities, _playerHitTargets, _playerHitTargetCount, _enemyHitTargets, _enemyHitTargetCount, _playerDamageRequests, _enemyDamageRequests, _meleeDespawnRequests);
                MeleeDespawnSystem.Tick(_entities, _meleeDespawnRequests);
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
                EnemySpawnSystem.Tick(_entities, _enemySpawnRequests, _rule.navMeshSampleDistance);
                ProjectileSpawnSystem.Tick(_entities, _straightProjectileSpawnRequests);
                ProjectileSpawnSystem.Tick(_entities, _arcProjectileSpawnRequests);
                MeleeSpawnSystem.Tick(_entities, _meleeSpawnRequests);
                LootSpawnSystem.Tick(_entities, _lootSpawnRequests, _lootRule.lifetimeTicks);
            }

            CommandSystem.ClearPlayerPresses(ref _commandBuffer.playerCommand);
        }

        /// <summary>
        /// 프레임 단위 입력을 읽어 명령 버퍼에 쌓는다. App이 Update에서 부른다.
        /// </summary>
        /// <param name="isSettingsOpen">설정 화면이 열려 있는지. 열려 있으면 ESC가 설정 화면을 닫는다</param>
        /// <param name="settingsCommands">ESC로 설정 화면을 닫는 명령을 넣을 목록</param>
        public void ReadInput(bool isSettingsOpen, List<SettingsCommand> settingsCommands)
        {
            if (!_isRoundReady)
            {
                return;
            }

            ReadEscapeInput(_commandBuffer, settingsCommands, isSettingsOpen, in _progress);
            ReadUpgradeInput(_commandBuffer, in _progress);
            if (_progress.isPaused)
            {
                ClearHeldPlayerInput(ref _commandBuffer.playerCommand);
            }
            else
            {
                ReadPlayerInput(ref _commandBuffer.playerCommand);
            }
        }

        /// <summary>
        /// 판의 View와 UI를 정해진 순서로 그리고 프레임 기록을 비운다. App이 LateUpdate에서 부른다.
        /// </summary>
        /// <param name="isSettingsOpen">설정 화면이 열려 있는지. 열려 있으면 일시정지 화면을 숨긴다</param>
        /// <param name="music">App이 이 씬에 만든 배경음악 실행 상태. 없으면 null</param>
        public void Present(bool isSettingsOpen, MusicViewContext music)
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
            EnemyViewSystem.Present(_entities.entities, _enemyStateChangedRecords, _enemyAttackCommittedRecords, _enemyDamagedRecords, _enemyViewPrefabs, _enemyAttackAnimations, _enemyAttackClips, isPaused, _viewContext.enemy);
            EnemyHealthGaugeViewSystem.Present(_entities.entities, _enemyHealthGaugePrefabs, _viewContext.camera.mainCamera, _viewContext.enemy);
            WorldInformationViewSystem.Present(_viewContext.camera.mainCamera, _viewContext.worldInformation);
            ProjectileViewSystem.Present(_entities.entities, _projectilePrefabs, isPaused, _viewContext.projectile);
            LootViewSystem.Present(_entities.entities, _lootPrefabs, isPaused, _viewContext.loot);
            HitEffectViewSystem.Present(_projectileHitRecords, _playerAttackCommittedRecords, _hitEffectPrefabs, isPaused, _viewContext.hitEffect);
            DamageTextViewSystem.Present(_enemyDamagedRecords, _viewContext.enemy.views, _viewContext.enemy.viewGenerations, _worldTextViewPrefab, _viewContext.camera.mainCamera, _viewContext.worldText.pendingTexts);
            LootTextViewSystem.Present(_lootPickedUpRecords, _worldTextViewPrefab, _viewContext.worldText.pendingTexts);
            LootSoundViewSystem.Present(_lootPickedUpRecords, _lootPrefabs, _viewContext.player.view != null ? _viewContext.player.view.soundEffectSource : null);
            WorldTextViewSystem.Present(_worldTextViewPrefab, _viewContext.camera.mainCamera, _viewContext.worldText);
            CameraViewSystem.Present(_viewContext.camera, _viewContext.player.view, _playerCameraPriority, _commandBuffer.playerCommand.cameraYawInput, _playerCameraYawDegreesPerSecond);
            if (music != null)
            {
                MusicViewSystem.Present(_entities.entities, _enemyHasOverlayHealthBar, music);
            }

            PresentUserInterface(isSettingsOpen);

            _playerStateChangedRecords.Clear();
            _playerAttackCommittedRecords.Clear();
            _enemyStateChangedRecords.Clear();
            _enemyAttackCommittedRecords.Clear();
            _projectileHitRecords.Clear();
            _enemyDamagedRecords.Clear();
            _lootPickedUpRecords.Clear();
        }

        private void Awake()
        {
            _playerActions = new PlayerActions();
            _playerActions.Global.Enable();
            _playerActions.UserInterface.Enable();
        }

        // 씬의 다른 오브젝트가 Awake를 마친 뒤에 App에 등록한다. App이 Initialize를 불러 판을 준비한다
        private void Start()
        {
            App app = App.Instance;
            if (app == null)
            {
                Debug.LogError("World >> App이 없어 판을 준비하지 않았다. 에디터라면 App 프리팹이 있는지 확인한다.");
                return;
            }

            app.RegisterWorld(this, _musicDefinition, _settingsPresenter);
        }

        private void OnDestroy()
        {
            App app = App.Instance;
            if (app != null)
            {
                app.UnregisterWorld(this);
            }
            // 일시정지 중에 씬이 사라져도 소리와 DOTween의 일시정지가 다음 씬에 남지 않게 한다
            ClearRound();
            _playerActions.Global.Disable();
            _playerActions.UserInterface.Disable();
            _playerActions.Dispose();
        }
    }
}
