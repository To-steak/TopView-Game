using TopView.Components;
using TopView.Definitions;
using TopView.Presentation.ViewContexts;
using TopView.Presentation.ViewSystems;
using TopView.Systems;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TopView.Core
{
    // 틱 순서와 호출 순서를 관리한다. 씬 로드 전에 스스로 만들어져 DDOL에 하나만 남는다.
    // 요소별 상태와 처리는 Runner.<요소>.cs에 나눠 두고, 이 파일에는 판 전체의 상태와 호출 순서만 둔다.
    public partial class Runner : MonoBehaviour
    {
#if UNITY_EDITOR
        // 이번 판의 World. Editor의 RunnerGizmos가 읽기만 한다. 판이 없으면 null이다
        public World EditorWorld => _world;
#endif

        private DefinitionCatalog _catalog;
        private World _world;
        private ViewContext _viewContext;
        // 마지막으로 판을 준비한 씬. 활성 씬이 바뀌면 새 판을 준비한다
        private int _sceneHandle;

        private const string DEFINITION_CATALOG_PATH = "Definition Catalog";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Create()
        {
            GameObject runnerObject = new GameObject(nameof(Runner));
            DontDestroyOnLoad(runnerObject);
            runnerObject.AddComponent<Runner>();
        }

        private void Awake()
        {
            _catalog = Resources.Load<DefinitionCatalog>(DEFINITION_CATALOG_PATH);
            if (_catalog == null)
            {
                Debug.LogError($"Runner >> Resources/{DEFINITION_CATALOG_PATH} 에셋이 없다.");
            }

            _playerActions = new PlayerActions();
            _playerActions.Global.Enable();
        }

        private void OnDestroy()
        {
            _playerActions.Global.Disable();
            _playerActions.Dispose();
        }

        private void FixedUpdate()
        {
            if (_world == null)
            {
                return;
            }

            bool isPlaying = _world.progress.phase == GamePhase.Playing;
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
                EnemyAttackStateSystem.Tick(_world.enemies, in _world.playerLocation, _world.enemyStateChangeRequests, _world.projectileSpawnRequests, _world.meleeSpawnRequests);
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
                EnemySpawnerSystem.Tick(in _world.rule, _world.enemySpawners, _world.enemySpawnerStates, _world.enemySpawnTemplates, _world.enemySpawnerActivationRequests, _world.enemySpawnRequests);
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
            int sceneHandle = SceneManager.GetActiveScene().handle;
            if (sceneHandle != _sceneHandle)
            {
                _sceneHandle = sceneHandle;
                StartRound();
            }

            if (_world == null)
            {
                return;
            }

            ReadPlayerInput(ref _world.commandBuffer.playerCommand);
            HandleSceneCommands(_world.commandBuffer.gameFlowCommands);
        }

        private void LateUpdate()
        {
            if (_world == null)
            {
                return;
            }

            PlayerViewSystem.Present(_world.players.entities, _world.playerStateChangedRecords, _world.playerAttackCommittedRecords, _playerViewPrefab, _playerWeaponModelPrefabs, _playerRangedLayerBlendInTicks, _viewContext.player);
            PlayerThrowPreviewViewSystem.Present(_world.players.entities, _viewContext.player.view, _playerThrowPreviewCircleHeightOffset);
            EnemyViewSystem.Present(_world.enemies.entities, _world.enemyStateChangedRecords, _world.enemyDamagedRecords, _enemyViewPrefabs, _viewContext.enemy);
            EnemyHealthGaugeViewSystem.Present(_world.enemies.entities, _enemyHealthGaugePrefabs, _viewContext.camera.mainCamera, _viewContext.enemy);
            WorldInformationViewSystem.Present(_viewContext.camera.mainCamera, _viewContext.worldInformation);
            ProjectileViewSystem.Present(_world.projectiles.entities, _projectilePrefabs, _viewContext.projectile);
            LootViewSystem.Present(_world.loots.entities, _lootPrefabs, _viewContext.loot);
            HitEffectViewSystem.Present(_world.projectileHitRecords, _hitEffectPrefabs, _viewContext.hitEffect);
            DamageTextViewSystem.Present(_world.enemyDamagedRecords, _viewContext.enemy.views, _viewContext.enemy.viewGenerations, _worldTextViewPrefab, _viewContext.camera.mainCamera, _viewContext.worldText.pendingTexts);
            LootTextViewSystem.Present(_world.lootPickedUpRecords, _worldTextViewPrefab, _viewContext.worldText.pendingTexts);
            WorldTextViewSystem.Present(_worldTextViewPrefab, _viewContext.camera.mainCamera, _viewContext.worldText);
            CameraViewSystem.Present(_viewContext.camera, _viewContext.player.view, _playerCameraPriority, _world.commandBuffer.playerCommand.cameraYawInput, _playerCameraYawDegreesPerSecond);
            if (_viewContext.music != null)
            {
                MusicViewSystem.Present(_viewContext.music);
            }

            PresentUserInterface();

            _world.playerStateChangedRecords.Clear();
            _world.playerAttackCommittedRecords.Clear();
            _world.enemyStateChangedRecords.Clear();
            _world.projectileHitRecords.Clear();
            _world.enemyDamagedRecords.Clear();
            _world.lootPickedUpRecords.Clear();
        }
    }
}
