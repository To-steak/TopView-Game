using TopView.Components;
using TopView.Definitions;
using TopView.Presentation.ViewContexts;
using TopView.Presentation.ViewSystems;
using TopView.Systems;
using UnityEngine;

namespace TopView.Core
{
    // 판이 있는 씬에 하나 배치해 그 판의 틱 순서와 호출 순서를 관리한다. 씬과 함께 생기고 사라진다.
    // 요소별 상태와 처리는 Runner.<요소>.cs에 나눠 두고, 이 파일에는 판 전체의 상태와 호출 순서만 둔다.
    public partial class Runner : MonoBehaviour
    {
#if UNITY_EDITOR
        // 이번 판의 World. Editor의 RunnerGizmos가 읽기만 한다. 판이 없으면 null이다
        public World EditorWorld => _world;
#endif

        // 한 판의 규칙과 저장소 용량
        [SerializeField] private GameSettingsDefinition _gameSettingsDefinition;

        private World _world;
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
            if (_world == null)
            {
                return;
            }

            // 이번 틱이 돌지 말지를 정하므로 맨 앞에 둔다
            PauseSystem.Tick(ref _world.progress, _world.commandBuffer.gameFlowCommands);
            // 강화 화면은 일시정지 중에 열리므로 isPlaying과 상관없이 돈다. 연 틱의 명령도 처리하도록 PauseSystem 뒤에 둔다
            PlayerUpgradeSystem.Tick(_world.players, ref _world.progress, _world.commandBuffer.playerUpgradeCommands);
            bool isPlaying = _world.progress.phase == GamePhase.Playing && !_world.progress.isPaused;
            DamageSystem.Tick(ref _world.progress, _world.players, _world.enemies, _world.playerDamageRequests, _world.enemyDamageRequests, _world.enemyDamagedRecords);
            if (isPlaying)
            {
                TransformSnapshotSystem.Tick(_world.players, _world.enemies, _world.projectiles);
                PlayerDeathSystem.Tick(_world.players, _world.playerStateChangedRecords);
                MeleeOwnerDeathSystem.Tick(_world.melees, _world.players, _world.meleeDespawnRequests);
                MeleeDespawnSystem.Tick(_world.melees, _world.meleeDespawnRequests);
                PlayerStateTimerSystem.Tick(_world.players);
                // 누름 입력으로 정해진 상태를 뒤의 셋이 함께 읽는다. 이동 입력과 공격을 반영한 뒤에 상태를 넘기므로 이 순서를 지킨다
                PlayerPressSystem.Tick(_world.players, in _world.commandBuffer.playerCommand, _world.playerStateChangeRequests);
                PlayerStateChangeSystem.Tick(_world.players, _world.playerStateChangeRequests, _world.playerStateChangedRecords);
                PlayerMoveInputSystem.Tick(_world.players, in _world.commandBuffer.playerCommand);
                PlayerAttackSystem.Tick(_world.players, _world.projectileSpawnRequests, _world.meleeSpawnRequests, _world.playerAttackCommittedRecords);
                PlayerStateSystem.Tick(_world.players, in _world.commandBuffer.playerCommand, _world.playerStateChangeRequests, _world.gameOverRequests);
                PlayerStateChangeSystem.Tick(_world.players, _world.playerStateChangeRequests, _world.playerStateChangedRecords);
                // 막기는 이동이 끝난 위치를 읽고, 조준은 막힌 뒤의 위치를 읽으므로 이 순서를 지킨다
                PlayerMovementSystem.Tick(_world.players);
                PlayerBlockSystem.Tick(_world.players, _world.enemies);
                PlayerAimSystem.Tick(_world.players, in _world.commandBuffer.playerCommand);
                // 미리보기는 이번 틱의 충전량과 이동·조준이 끝난 위치와 회전을 읽으므로 조준 뒤에 둔다
                PlayerThrowPreviewSystem.Tick(_world.players);
                PlayerStaminaSystem.Tick(_world.players);
                // 획득은 이동이 끝난 위치를 읽고, 이번 틱 피해가 반영된 체력을 보므로 이 자리에 둔다
                LootPickupSystem.Tick(_world.loots, _world.players, ref _world.progress, _world.lootRule.pickupRadius, _world.lootDespawnRequests, _world.lootPickedUpRecords);
                LootDespawnSystem.Tick(_world.loots, _world.lootDespawnRequests);
                LootLifetimeSystem.Tick(_world.loots, _world.lootDespawnRequests);
                LootDespawnSystem.Tick(_world.loots, _world.lootDespawnRequests);
                EnemyDeathSystem.Tick(_world.enemies, _world.enemyStateChangedRecords);
                EnemyStateTimerSystem.Tick(_world.enemies);
                PlayerLocationSystem.Tick(_world.players, ref _world.playerLocation);
                // 공격 상태는 상태 시간이 0이면 건너뛰므로, 공격이 끝나 Idle이 된 적과 막 공격에 들어온 적 모두 다음 틱부터 움직이도록 이 순서를 지킨다
                EnemyBehaviorSystem.Tick(_world.enemies, in _world.playerLocation, _world.random, _world.enemyStateChangeRequests);
                EnemyStateChangeSystem.Tick(_world.enemies, _world.enemyStateChangeRequests, _world.enemyStateChangedRecords);
                EnemyAttackStateSystem.Tick(_world.enemies, in _world.playerLocation, _world.enemyStateChangeRequests, _world.projectileSpawnRequests, _world.meleeSpawnRequests, _world.enemyAttackCommittedRecords);
                EnemyStateChangeSystem.Tick(_world.enemies, _world.enemyStateChangeRequests, _world.enemyStateChangedRecords);
                EnemyDespawnSystem.Tick(_world.enemies, _world.gameOverRequests, _world.random, _world.lootSpawnRequests);
                EnemyNavigationSystem.Tick(_world.enemies, _world.navigationPath, _world.rule.navMeshSampleDistance);
                ProjectileSystem.Tick(_world.projectiles, _world.projectileDespawnRequests);
                ProjectileDespawnSystem.Tick(_world.projectiles, _world.projectileDespawnRequests);
                HitTargetSystem.Tick(_world.players, _world.enemies, _world.playerHitTargets, ref _world.playerHitTargetCount, _world.enemyHitTargets, ref _world.enemyHitTargetCount);
                ProjectileHitSystem.Tick(_world.projectiles, _world.playerHitTargets, _world.playerHitTargetCount, _world.enemyHitTargets, _world.enemyHitTargetCount, _world.playerDamageRequests, _world.enemyDamageRequests, _world.projectileHitRecords, _world.projectileDespawnRequests);
                ProjectileDespawnSystem.Tick(_world.projectiles, _world.projectileDespawnRequests);
                MeleeHitSystem.Tick(_world.melees, _world.playerHitTargets, _world.playerHitTargetCount, _world.enemyHitTargets, _world.enemyHitTargetCount, _world.playerDamageRequests, _world.enemyDamageRequests, _world.meleeDespawnRequests);
                MeleeDespawnSystem.Tick(_world.melees, _world.meleeDespawnRequests);
            }

            GameRuleSystem.Tick(ref _world.progress, _world.gameOverRequests);
            if (isPlaying)
            {
                PlayerDetectorSystem.Tick(_world.playerDetectors, _world.playerDetectorStates, in _world.playerLocation, _world.enemySpawnerActivationRequests);
                // 레벨이 오른 틱에 생기는 적도 새 레벨로 나오도록 스포너보다 먼저 둔다
                EnemyLevelSystem.Tick(_world.rule.enemyLevelUpIntervalTicks, ref _world.progress);
                EnemySpawnerSystem.Tick(_world.progress.enemyLevel, _world.enemySpawners, _world.enemySpawnerStates, _world.enemySpawnTemplates, _world.enemySpawnerActivationRequests, _world.enemySpawnRequests);
                PlayerSpawnSystem.Tick(_world.players, _world.playerSpawnRequests);
                EnemySpawnSystem.Tick(_world.enemies, _world.enemySpawnRequests, _world.rule.navMeshSampleDistance);
                ProjectileSpawnSystem.Tick(_world.projectiles, _world.projectileSpawnRequests);
                MeleeSpawnSystem.Tick(_world.melees, _world.meleeSpawnRequests);
                LootSpawnSystem.Tick(_world.loots, _world.lootSpawnRequests, _world.lootRule.lifetimeTicks);
            }

            CommandSystem.ClearPlayerPresses(ref _world.commandBuffer.playerCommand);
        }

        private void Update()
        {
            if (_world == null)
            {
                return;
            }

            ReadEscapeInput(_world.commandBuffer, in _world.progress);
            ReadUpgradeInput(_world.commandBuffer, in _world.progress);
            if (_world.progress.isPaused)
            {
                ClearHeldPlayerInput(ref _world.commandBuffer.playerCommand);
            }
            else
            {
                ReadPlayerInput(ref _world.commandBuffer.playerCommand);
            }
            HandleSettingsCommands(_world.commandBuffer.settingsCommands);
            // 씬을 바꾸면 판을 비우므로 마지막에 둔다
            HandleSceneCommands(_world.commandBuffer.gameFlowCommands, in _world.progress);
        }

        private void LateUpdate()
        {
            if (_world == null)
            {
                return;
            }

            bool isPaused = _world.progress.isPaused;
            // 이번 프레임에 일시정지가 걸리거나 풀리면 소리와 DOTween에 먼저 반영한다
            PauseViewSystem.Present(isPaused, _viewContext.pause);
            CursorViewSystem.Present(isPaused, _gameSettingsDefinition.crosshairCursor, _viewContext.cursor);
            PlayerViewSystem.Present(_world.players.entities, _world.playerStateChangedRecords, _world.playerAttackCommittedRecords, _playerViewPrefab, _playerWeaponModelPrefabs, _playerRangedLayerBlendInTicks, isPaused, _viewContext.player);
            PlayerThrowPreviewViewSystem.Present(_world.players.entities, _viewContext.player.view, _playerThrowPreviewCircleHeightOffset);
            EnemyViewSystem.Present(_world.enemies.entities, _world.enemyStateChangedRecords, _world.enemyAttackCommittedRecords, _world.enemyDamagedRecords, _enemyViewPrefabs, isPaused, _viewContext.enemy);
            EnemyHealthGaugeViewSystem.Present(_world.enemies.entities, _enemyHealthGaugePrefabs, _viewContext.camera.mainCamera, _viewContext.enemy);
            WorldInformationViewSystem.Present(_viewContext.camera.mainCamera, _viewContext.worldInformation);
            ProjectileViewSystem.Present(_world.projectiles.entities, _projectilePrefabs, isPaused, _viewContext.projectile);
            LootViewSystem.Present(_world.loots.entities, _lootPrefabs, isPaused, _viewContext.loot);
            HitEffectViewSystem.Present(_world.projectileHitRecords, _world.playerAttackCommittedRecords, _hitEffectPrefabs, isPaused, _viewContext.hitEffect);
            DamageTextViewSystem.Present(_world.enemyDamagedRecords, _viewContext.enemy.views, _viewContext.enemy.viewGenerations, _worldTextViewPrefab, _viewContext.camera.mainCamera, _viewContext.worldText.pendingTexts);
            LootTextViewSystem.Present(_world.lootPickedUpRecords, _worldTextViewPrefab, _viewContext.worldText.pendingTexts);
            LootSoundViewSystem.Present(_world.lootPickedUpRecords, _lootPrefabs, _viewContext.player.view != null ? _viewContext.player.view.soundEffectSource : null);
            WorldTextViewSystem.Present(_worldTextViewPrefab, _viewContext.camera.mainCamera, _viewContext.worldText);
            CameraViewSystem.Present(_viewContext.camera, _viewContext.player.view, _playerCameraPriority, _world.commandBuffer.playerCommand.cameraYawInput, _playerCameraYawDegreesPerSecond);
            if (_viewContext.music != null)
            {
                MusicViewSystem.Present(_world.enemies.entities, _viewContext.music);
            }

            PresentUserInterface();
            if (_viewContext.userInterfaceSound != null)
            {
                UserInterfaceSoundViewSystem.Present(_userInterfaceSoundRequests, _viewContext.userInterfaceSound);
            }
            _userInterfaceSoundRequests.Clear();

            _world.playerStateChangedRecords.Clear();
            _world.playerAttackCommittedRecords.Clear();
            _world.enemyStateChangedRecords.Clear();
            _world.enemyAttackCommittedRecords.Clear();
            _world.projectileHitRecords.Clear();
            _world.enemyDamagedRecords.Clear();
            _world.lootPickedUpRecords.Clear();
        }
    }
}
