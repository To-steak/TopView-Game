using System.Collections.Generic;
using TopView.Authoring;
using TopView.Components;
using TopView.Definitions;
using TopView.Entities;
using TopView.Presentation;
using TopView.Presentation.Boss.ViewSystems;
using TopView.Presentation.Camera.ViewSystems;
using TopView.Presentation.Camera.Views;
using TopView.Presentation.Enemy.ViewSystems;
using TopView.Presentation.Enemy.Views;
using TopView.Presentation.Player.ViewSystems;
using TopView.Presentation.Projectile.ViewSystems;
using TopView.Presentation.Projectile.Views;
using TopView.Requests;
using TopView.Systems;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TopView.Core
{
    // 틱 순서와 호출 순서를 관리한다. 씬 로드 전에 스스로 만들어져 DDOL에 하나만 남는다.
    // 요소별 상태와 처리는 Runner.<요소>.cs에 나눠 두고, 이 파일에는 판 전체의 상태와 호출 순서만 둔다.
    public partial class Runner : MonoBehaviour
    {
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
            DamageSystem.Tick(ref _world.progress, _world.players, _world.enemies, _world.bosses, _world.damageRequests);
            if (isPlaying)
            {
                PlayerStateSystem.Tick(_world.players, in _world.commandBuffer.playerCommand, _world.playerStateChangedRecords, _world.projectileSpawnRequests, _world.gameOverRequests);
                PlayerMovementSystem.Tick(_world.players, in _world.commandBuffer.playerCommand, _world.enemies, _world.bosses);
                PlayerStaminaSystem.Tick(_world.players);
                EnemyBehaviorSystem.Tick(_world.enemies, _world.players, _world.random, _world.enemyStateChangedRecords, _world.projectileSpawnRequests, _world.damageRequests);
                BossBehaviorSystem.Tick(_world.bosses, _world.players, _world.random, _world.bossStateChangedRecords, _world.projectileSpawnRequests, _world.damageRequests, _world.gameOverRequests);
                EnemyNavigationSystem.Tick(_world.enemies, _world.navigationPath);
                ProjectileSystem.Tick(_world.projectiles);
                HitSystem.Tick(_world.projectiles, _world.players, _world.enemies, _world.bosses, _world.damageRequests, _world.projectileHitRecords);
            }

            GameRuleSystem.Tick(ref _world.progress, _world.gameOverRequests);
            if (isPlaying)
            {
                WaveSystem.Tick(in _world.rule, _world.waves, _world.enemySpawnPointsByKind, _world.enemySpawnTemplates, in _world.bossSpawnTemplate, ref _world.progress, _world.random, _world.enemySpawnRequests, _world.bossSpawnRequests);
                PlayerSpawnSystem.Tick(_world.players, _world.playerSpawnRequests);
                EnemySpawnSystem.Tick(_world.enemies, _world.enemySpawnRequests);
                BossSpawnSystem.Tick(_world.bosses, _world.bossSpawnRequests, _world.players);
                ProjectileSpawnSystem.Tick(_world.projectiles, _world.projectileSpawnRequests);
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

            PlayerViewSystem.Present(_world.players, _world.playerStateChangedRecords, _playerViewPrefab, _playerWeaponModelPrefabs, _viewContext);
            EnemyViewSystem.Present(_world.enemies, _world.enemyStateChangedRecords, _enemyViewPrefabs, _viewContext);
            BossViewSystem.Present(_world.bosses, _world.bossStateChangedRecords, _bossViewPrefab, _viewContext);
            ProjectileViewSystem.Present(_world.projectiles, _projectilePrefabs, _projectilePitchSpeeds, _world.projectileHitRecords, _hitEffectPrefabs, _viewContext);
            CameraViewSystem.Present(_viewContext, _playerCameraPriority);

            PresentUserInterface();

            _world.playerStateChangedRecords.Clear();
            _world.enemyStateChangedRecords.Clear();
            _world.bossStateChangedRecords.Clear();
            _world.projectileHitRecords.Clear();
        }

        // 새 World를 만들고 Definition과 씬의 Authoring 값을 채운 뒤, 씬의 View와 Presenter를 찾아 연결한다.
        private void StartRound()
        {
            ClearRound();

            // Player Spawner Authoring이 없는 씬(메뉴 등)은 판을 하지 않는 씬이다
            if (!TryFindPlayerSpawner(out PlayerSpawnerAuthoring playerSpawner))
            {
                return;
            }

            if (_catalog == null)
            {
                Debug.LogError("Runner >> Definition Catalog가 없어 판을 시작할 수 없다.");
                return;
            }

            GameSettingsDefinition gameSettings = _catalog.gameSettings;
            if (gameSettings == null)
            {
                Debug.LogError("Runner >> Definition Catalog에 Game Settings Definition이 연결되어 있지 않아 판을 시작할 수 없다.");
                return;
            }

            List<GameObject> projectilePrefabs = new List<GameObject>();
            List<float> projectilePitchSpeeds = new List<float>();
            List<GameObject> hitEffectPrefabs = new List<GameObject>();

            if (!TryReadPlayer(playerSpawner, projectilePrefabs, projectilePitchSpeeds, hitEffectPrefabs, out PlayerDefinition playerDefinition, out PlayerSpawnRequest playerSpawnRequest, out GameObject[] weaponModelPrefabs))
            {
                return;
            }

            if (gameSettings.enemyCapacityPerKind <= 0 || gameSettings.projectileCapacity <= 0)
            {
                Debug.LogError("Runner >> Game Settings Definition의 Enemy Capacity Per Kind나 Projectile Capacity가 0 이하라 판을 시작할 수 없다.");
                return;
            }

            WaveComponent[] waves = (WaveComponent[])gameSettings.waves.Clone();
            if (!TryAssignWaveEnemyKinds(waves, gameSettings.waveEnemies, out EnemyDefinition[] enemyDefinitions))
            {
                return;
            }

            int enemyKindCount = enemyDefinitions.Length;
            if (!TryReadEnemies(enemyDefinitions, projectilePrefabs, projectilePitchSpeeds, hitEffectPrefabs, out EnemySpawnRequest[] enemySpawnTemplates, out GameObject[] enemyViewPrefabs, out Vector3[] gaugeOffsets, out Color[] gaugeColors))
            {
                return;
            }

            if (!TryReadBoss(projectilePrefabs, projectilePitchSpeeds, hitEffectPrefabs, out BossDefinition bossDefinition, out BossSpawnRequest bossSpawnTemplate))
            {
                return;
            }

            World world = new World();
            world.rule = gameSettings.rule;
            world.waves = waves;
            world.enemySpawnPointsByKind = CollectEnemySpawnPoints(enemyDefinitions);
            world.random = new System.Random(world.rule.randomSeed);
            world.players = new PlayerStorage(PLAYER_CAPACITY);
            world.playerSpawnRequests.Enqueue(playerSpawnRequest);
            world.enemies = new EnemyStorage(enemyKindCount, gameSettings.enemyCapacityPerKind);
            world.enemySpawnTemplates = enemySpawnTemplates;
            world.bosses = new BossStorage(BOSS_CAPACITY);
            world.bossSpawnTemplate = bossSpawnTemplate;
            world.projectiles = new ProjectileStorage(gameSettings.projectileCapacity);

            world.progress = new GameProgressComponent
            {
                phase = GamePhase.Playing,
                remainingTicks = world.rule.timeLimitTicks,
                score = 0,
                isWaveStarted = new bool[world.waves.Length],
                isEnemyKindSpawning = new bool[enemyKindCount],
                enemyKindSpawnRemainingTicks = new int[enemyKindCount]
            };

            Application.targetFrameRate = gameSettings.targetFrameRate;
            _world = world;
            _playerViewPrefab = playerDefinition.viewPrefab;
            _playerCameraPriority = playerDefinition.cameraPriority;
            _playerWeaponModelPrefabs = weaponModelPrefabs;
            _enemyViewPrefabs = enemyViewPrefabs;
            _bossViewPrefab = bossDefinition.viewPrefab;
            _projectilePrefabs = projectilePrefabs.ToArray();
            _projectilePitchSpeeds = projectilePitchSpeeds.ToArray();
            _hitEffectPrefabs = hitEffectPrefabs.ToArray();
            _viewContext = CreateViewContext(enemyKindCount);

            BindPresenters(playerDefinition, gaugeOffsets, gaugeColors);
        }

        // 이전 판의 World, View 실행 상태, Presenter 연결을 비운다.
        private void ClearRound()
        {
            _world = null;
            _viewContext = null;
            _gameOverPresenter = null;
            _remainingTimePresenter = null;
            _scorePresenter = null;
            _playerHealthPresenter = null;
            _ammoPresenter = null;
            _playerManaGaugePresenter = null;
            _bossHealthPresenter = null;
            _enemyHealthGaugePresenter = null;
        }

        // 이번 판의 저장소 크기와 View 표에 맞춰 View 실행 상태를 만들고 씬의 카메라를 찾는다.
        private ViewContext CreateViewContext(int enemyKindCount)
        {
            ViewContext viewContext = new ViewContext
            {
                mainCamera = UnityEngine.Camera.main,
                playerCameraView = FindAnyObjectByType<PlayerCameraView>(),
                enemyViews = new EnemyView[_world.enemies.entities.Length],
                enemyViewGenerations = new int[_world.enemies.entities.Length],
                enemyViewPools = new Stack<EnemyView>[enemyKindCount],
                enemyTriggerHashesByKind = new int[enemyKindCount][],
                projectileViews = new ProjectileView[_world.projectiles.entities.Length],
                projectileViewGenerations = new int[_world.projectiles.entities.Length],
                projectileViewIndices = new int[_world.projectiles.entities.Length],
                projectileViewPools = new Stack<ProjectileView>[_projectilePrefabs.Length],
                hitEffectPools = new Stack<ParticleSystem>[_hitEffectPrefabs.Length]
            };
            for (int kind = 0; kind < enemyKindCount; kind++)
            {
                viewContext.enemyViewPools[kind] = new Stack<EnemyView>();
            }
            for (int viewIndex = 0; viewIndex < _projectilePrefabs.Length; viewIndex++)
            {
                viewContext.projectileViewPools[viewIndex] = new Stack<ProjectileView>();
            }
            for (int effectIndex = 0; effectIndex < _hitEffectPrefabs.Length; effectIndex++)
            {
                viewContext.hitEffectPools[effectIndex] = new Stack<ParticleSystem>();
            }
            if (viewContext.mainCamera == null)
            {
                Debug.LogError("Runner >> 씬에 MainCamera 태그를 가진 카메라가 없다.");
            }
            if (viewContext.playerCameraView == null)
            {
                Debug.LogError("Runner >> 씬에 PlayerCameraView가 없다.");
            }

            return viewContext;
        }
    }
}
