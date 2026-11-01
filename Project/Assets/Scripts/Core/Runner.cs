using System.Collections.Generic;
using TopView.Authoring;
using TopView.Commands;
using TopView.Components;
using TopView.Definitions;
using TopView.Entities;
using TopView.Presentation;
using TopView.Presentation.Boss.ViewSystems;
using TopView.Presentation.Boss.Views;
using TopView.Presentation.Camera.ViewSystems;
using TopView.Presentation.Camera.Views;
using TopView.Presentation.Enemy.ViewSystems;
using TopView.Presentation.Enemy.Views;
using TopView.Presentation.Player.ViewSystems;
using TopView.Presentation.Player.Views;
using TopView.Presentation.Projectile.ViewSystems;
using TopView.Presentation.Projectile.Views;
using TopView.Requests;
using TopView.Systems;
using TopView.UserInterface.Ammo;
using TopView.UserInterface.BossHealth;
using TopView.UserInterface.GameOver;
using TopView.UserInterface.PlayerHealth;
using TopView.UserInterface.RemainingTime;
using TopView.UserInterface.Score;
using TopView.UserInterface.WorldGauge;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace TopView.Core
{
    // 틱 순서와 호출 순서를 관리한다. 씬 로드 전에 스스로 만들어져 DDOL에 하나만 남는다.
    public partial class Runner : MonoBehaviour
    {
        private DefinitionCatalog _catalog;
        private PlayerActions _playerActions;
        private World _world;
        private ViewContext _viewContext;
        private GameObject _playerViewPrefab;
        private int _playerCameraPriority;
        private GameObject[] _playerWeaponModelPrefabs;
        private GameObject[] _enemyViewPrefabs;
        private GameObject _bossViewPrefab;
        // 투사체 View 표와 피격 이펙트 표. 판을 준비할 때 Definition에서 모은다
        private GameObject[] _projectilePrefabs;
        private float[] _projectilePitchSpeeds;
        private GameObject[] _hitEffectPrefabs;
        private GameOverPresenter _gameOverPresenter;
        private RemainingTimePresenter _remainingTimePresenter;
        private ScorePresenter _scorePresenter;
        private PlayerHealthPresenter _playerHealthPresenter;
        private AmmoPresenter _ammoPresenter;
        private PlayerManaGaugePresenter _playerManaGaugePresenter;
        private BossHealthPresenter _bossHealthPresenter;
        private EnemyHealthGaugePresenter _enemyHealthGaugePresenter;
        // 마지막으로 판을 준비한 씬. 활성 씬이 바뀌면 새 판을 준비한다
        private int _sceneHandle;

        private const string DEFINITION_CATALOG_PATH = "Definition Catalog";
        private const int PLAYER_CAPACITY = 1;
        private const int BOSS_CAPACITY = 1;

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
            PlayerDamageSystem.Tick(_world.progress.phase, _world.players, _world.playerDamageRequests, _world.playerStateChangedRecords);
            EnemyDamageSystem.Tick(ref _world.progress, _world.enemies, _world.bosses, _world.enemyDamageRequests, _world.bossDamageRequests, _world.enemyStateChangedRecords, _world.bossStateChangedRecords);
            if (isPlaying)
            {
                PlayerStateSystem.Tick(_world.players, in _world.commandBuffer.playerCommand, _world.playerStateChangedRecords, _world.projectileSpawnRequests, _world.gameOverRequests);
                PlayerMovementSystem.Tick(_world.players, in _world.commandBuffer.playerCommand, _world.enemies, _world.bosses);
                PlayerStaminaSystem.Tick(_world.players);
                EnemyBehaviorSystem.Tick(_world.enemies, _world.players, _world.random, _world.enemyStateChangedRecords, _world.projectileSpawnRequests, _world.playerDamageRequests);
                BossBehaviorSystem.Tick(_world.bosses, _world.players, _world.random, _world.bossStateChangedRecords, _world.projectileSpawnRequests, _world.playerDamageRequests, _world.gameOverRequests);
                EnemyNavigationSystem.Tick(_world.enemies, _world.navigationPath);
                ProjectileSystem.Tick(_world.projectiles);
                HitSystem.Tick(_world.projectiles, _world.players, _world.enemies, _world.bosses, _world.playerDamageRequests, _world.enemyDamageRequests, _world.bossDamageRequests, _world.projectileHitRecords);
            }

            GameRuleSystem.Tick(ref _world.progress, _world.gameOverRequests);
            if (isPlaying)
            {
                WaveSystem.Tick(in _world.rule, _world.waves, _world.enemySpawnPointsByKind, _world.enemySpawnTemplates, in _world.bossSpawnTemplate, ref _world.progress, _world.random, _world.enemySpawnRequests, _world.bossSpawnRequests);
                SpawnSystem.Tick(_world.players, _world.playerSpawnRequests, _world.enemies, _world.enemySpawnRequests, _world.bosses, _world.bossSpawnRequests, _world.projectiles, _world.projectileSpawnRequests);
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

            // Player Spawner Authoring이 없는 씬(메뉴 등)은 판을 하지 않는 씬이다
            PlayerSpawnerAuthoring[] playerSpawners = FindObjectsByType<PlayerSpawnerAuthoring>(FindObjectsSortMode.None);
            if (playerSpawners.Length == 0)
            {
                return;
            }

            if (playerSpawners.Length > 1)
            {
                Debug.LogError($"Runner >> 씬에 Player Spawner Authoring이 하나여야 하는데 {playerSpawners.Length}개라 판을 시작할 수 없다.");
                return;
            }

            PlayerSpawnerAuthoring playerSpawner = playerSpawners[0];
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

            PlayerDefinition playerDefinition = playerSpawner.definition;
            if (playerDefinition == null)
            {
                Debug.LogError("Runner >> Player Spawner Authoring에 Player Definition이 연결되어 있지 않아 판을 시작할 수 없다.");
                return;
            }

            List<GameObject> projectilePrefabs = new List<GameObject>();
            List<float> projectilePitchSpeeds = new List<float>();
            List<GameObject> hitEffectPrefabs = new List<GameObject>();

            if (!TryReadWeapons(playerDefinition, projectilePrefabs, projectilePitchSpeeds, hitEffectPrefabs, out WeaponComponent[] weapons, out GameObject[] weaponModelPrefabs))
            {
                return;
            }

            if (playerDefinition.viewPrefab == null || playerDefinition.viewPrefab.GetComponent<PlayerView>() == null)
            {
                Debug.LogError("Runner >> Player Definition의 View Prefab이 없거나 PlayerView가 붙어 있지 않아 판을 시작할 수 없다.");
                return;
            }

            if (gameSettings.enemyCapacityPerKind <= 0 || gameSettings.projectileCapacity <= 0 || playerDefinition.meleeHitCapacity <= 0 || playerDefinition.cameraPriority <= 0)
            {
                Debug.LogError("Runner >> Game Settings Definition의 Enemy Capacity Per Kind·Projectile Capacity나 Player Definition의 Melee Hit Capacity·Camera Priority가 0 이하라 판을 시작할 수 없다.");
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

            if (!TryFindBossSpawner(out BossSpawnerAuthoring bossSpawner))
            {
                return;
            }

            BossDefinition bossDefinition = bossSpawner.definition;
            if (bossDefinition == null || bossDefinition.viewPrefab == null || bossDefinition.viewPrefab.GetComponent<BossView>() == null)
            {
                Debug.LogError("Runner >> Boss Spawner Authoring에 Boss Definition이 없거나, View Prefab이 없거나 BossView가 붙어 있지 않아 판을 시작할 수 없다.");
                return;
            }

            EnemyAttackComponent[] bossAttacks = (EnemyAttackComponent[])bossDefinition.attacks.Clone();
            if (!TryAssignAttackViews(bossDefinition.name, bossAttacks, bossDefinition.attackProjectilePrefabs, bossDefinition.attackHitEffectPrefabs, bossDefinition.attackProjectilePitchSpeeds, projectilePrefabs, projectilePitchSpeeds, hitEffectPrefabs))
            {
                return;
            }

            World world = new World();
            world.rule = gameSettings.rule;
            world.waves = waves;
            world.enemySpawnPointsByKind = CollectEnemySpawnPoints(enemyDefinitions);
            world.random = new System.Random(world.rule.randomSeed);
            world.players = new PlayerStorage(PLAYER_CAPACITY);
            world.playerSpawnRequests.Enqueue(new PlayerSpawnRequest
            {
                position = playerSpawner.transform.position,
                rotation = Quaternion.identity,
                movement = playerDefinition.movement,
                vitals = playerDefinition.vitals,
                action = playerDefinition.action,
                weapons = weapons,
                meleeHitCapacity = playerDefinition.meleeHitCapacity
            });
            world.enemies = new EnemyStorage(enemyKindCount, gameSettings.enemyCapacityPerKind);
            world.enemySpawnTemplates = enemySpawnTemplates;
            world.bosses = new BossStorage(BOSS_CAPACITY);
            world.bossSpawnTemplate = new BossSpawnRequest
            {
                position = bossSpawner.transform.position,
                stats = bossDefinition.stats,
                behavior = bossDefinition.behavior,
                attacks = bossAttacks
            };
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

            _viewContext = new ViewContext
            {
                mainCamera = UnityEngine.Camera.main,
                playerCameraView = FindAnyObjectByType<PlayerCameraView>(),
                enemyViews = new EnemyView[world.enemies.entities.Length],
                enemyViewGenerations = new int[world.enemies.entities.Length],
                enemyViewPools = new Stack<EnemyView>[enemyKindCount],
                enemyTriggerHashesByKind = new int[enemyKindCount][],
                projectileViews = new ProjectileView[world.projectiles.entities.Length],
                projectileViewGenerations = new int[world.projectiles.entities.Length],
                projectileViewIndices = new int[world.projectiles.entities.Length],
                projectileViewPools = new Stack<ProjectileView>[_projectilePrefabs.Length],
                hitEffectPools = new Stack<ParticleSystem>[_hitEffectPrefabs.Length]
            };
            for (int kind = 0; kind < enemyKindCount; kind++)
            {
                _viewContext.enemyViewPools[kind] = new Stack<EnemyView>();
            }
            for (int viewIndex = 0; viewIndex < _projectilePrefabs.Length; viewIndex++)
            {
                _viewContext.projectileViewPools[viewIndex] = new Stack<ProjectileView>();
            }
            for (int effectIndex = 0; effectIndex < _hitEffectPrefabs.Length; effectIndex++)
            {
                _viewContext.hitEffectPools[effectIndex] = new Stack<ParticleSystem>();
            }
            if (_viewContext.mainCamera == null)
            {
                Debug.LogError("Runner >> 씬에 MainCamera 태그를 가진 카메라가 없다.");
            }
            if (_viewContext.playerCameraView == null)
            {
                Debug.LogError("Runner >> 씬에 PlayerCameraView가 없다.");
            }

            BindPresenters(playerDefinition, gaugeOffsets, gaugeColors);
        }

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

        // 보스가 아닌 웨이브마다 Game Settings Definition의 웨이브 적 목록에서 Enemy Definition을 읽어 적 종류 번호를 매긴다. 번호는 웨이브 순서에서 처음 나온 순서다. 칸이 모자라거나 비어 있으면 false
        private static bool TryAssignWaveEnemyKinds(WaveComponent[] waves, EnemyDefinition[] waveEnemies, out EnemyDefinition[] enemyDefinitions)
        {
            List<EnemyDefinition> kinds = new List<EnemyDefinition>();
            enemyDefinitions = null;

            for (int waveIndex = 0; waveIndex < waves.Length; waveIndex++)
            {
                ref WaveComponent wave = ref waves[waveIndex];
                if (wave.isBoss)
                {
                    continue;
                }

                EnemyDefinition definition = waveEnemies != null && waveIndex < waveEnemies.Length ? waveEnemies[waveIndex] : null;
                if (definition == null)
                {
                    Debug.LogError($"Runner >> Game Settings Definition의 웨이브 {waveIndex}번에 적 종류(Wave Enemies)가 없어 판을 시작할 수 없다.");
                    return false;
                }

                wave.enemyKindIndex = kinds.IndexOf(definition);
                if (wave.enemyKindIndex < 0)
                {
                    wave.enemyKindIndex = kinds.Count;
                    kinds.Add(definition);
                }
            }

            enemyDefinitions = kinds.ToArray();
            return true;
        }

        // 적 종류별 Enemy Definition에서 생성 요청 템플릿과 View·게이지 값을 꺼낸다. 값이 빠져 있으면 false
        private static bool TryReadEnemies(EnemyDefinition[] enemyDefinitions, List<GameObject> projectilePrefabs, List<float> projectilePitchSpeeds, List<GameObject> hitEffectPrefabs, out EnemySpawnRequest[] spawnTemplates, out GameObject[] viewPrefabs, out Vector3[] gaugeOffsets, out Color[] gaugeColors)
        {
            int enemyKindCount = enemyDefinitions.Length;
            spawnTemplates = new EnemySpawnRequest[enemyKindCount];
            viewPrefabs = new GameObject[enemyKindCount];
            gaugeOffsets = new Vector3[enemyKindCount];
            gaugeColors = new Color[enemyKindCount];

            for (int kind = 0; kind < enemyKindCount; kind++)
            {
                EnemyDefinition definition = enemyDefinitions[kind];
                if (definition.viewPrefab == null || definition.viewPrefab.GetComponent<EnemyView>() == null)
                {
                    Debug.LogError($"Runner >> {definition.name}의 View Prefab이 없거나 EnemyView가 붙어 있지 않아 판을 시작할 수 없다.");
                    return false;
                }

                if (definition.navigation.navMeshSampleDistance <= 0f || definition.navigation.maxPathCorners <= 0)
                {
                    Debug.LogError($"Runner >> {definition.name}의 Nav Mesh Sample Distance나 Max Path Corners가 0 이하라 판을 시작할 수 없다.");
                    return false;
                }

                EnemyAttackComponent[] attacks = (EnemyAttackComponent[])definition.attacks.Clone();
                if (!TryAssignAttackViews(definition.name, attacks, definition.attackProjectilePrefabs, definition.attackHitEffectPrefabs, definition.attackProjectilePitchSpeeds, projectilePrefabs, projectilePitchSpeeds, hitEffectPrefabs))
                {
                    return false;
                }

                spawnTemplates[kind] = new EnemySpawnRequest
                {
                    enemyKindIndex = kind,
                    stats = definition.stats,
                    behavior = definition.behavior,
                    navigation = definition.navigation,
                    attacks = attacks
                };
                viewPrefabs[kind] = definition.viewPrefab;
                gaugeOffsets[kind] = definition.healthGaugeOffset;
                gaugeColors[kind] = definition.healthGaugeColor;
            }

            return true;
        }

        // Player Definition의 무기 목록에서 엔티티에 담을 값과 View가 쓸 모델 프리팹을 꺼내고, 투사체 View와 피격 이펙트를 표에 올린다. 빈 칸이 있으면 false
        private static bool TryReadWeapons(PlayerDefinition playerDefinition, List<GameObject> projectilePrefabs, List<float> projectilePitchSpeeds, List<GameObject> hitEffectPrefabs, out WeaponComponent[] weapons, out GameObject[] weaponModelPrefabs)
        {
            WeaponDefinition[] weaponDefinitions = playerDefinition.weapons;
            weapons = new WeaponComponent[weaponDefinitions.Length];
            weaponModelPrefabs = new GameObject[weaponDefinitions.Length];

            for (int weaponIndex = 0; weaponIndex < weaponDefinitions.Length; weaponIndex++)
            {
                WeaponDefinition definition = weaponDefinitions[weaponIndex];
                if (definition == null)
                {
                    Debug.LogError($"Runner >> Player Definition의 무기 {weaponIndex}번 칸이 비어 있어 판을 시작할 수 없다.");
                    return false;
                }

                weapons[weaponIndex] = definition.weapon;
                weaponModelPrefabs[weaponIndex] = definition.modelPrefab;

                bool hasProjectile = definition.weapon.kind != WeaponKind.Melee;
                if (!TryAssignProjectileViews(definition.name, hasProjectile, definition.projectilePrefab, definition.hitEffectPrefab, 0f, ref weapons[weaponIndex].projectile, projectilePrefabs, projectilePitchSpeeds, hitEffectPrefabs))
                {
                    return false;
                }
            }

            return true;
        }

        // 적·보스 공격 목록의 투사체마다 View와 피격 이펙트를 표에 올린다. Body가 아닌 공격의 프리팹이 없으면 false
        private static bool TryAssignAttackViews(string definitionName, EnemyAttackComponent[] attacks, GameObject[] attackProjectilePrefabs, GameObject[] attackHitEffectPrefabs, float[] attackPitchSpeeds, List<GameObject> projectilePrefabs, List<float> projectilePitchSpeeds, List<GameObject> hitEffectPrefabs)
        {
            for (int attackIndex = 0; attackIndex < attacks.Length; attackIndex++)
            {
                bool hasProjectile = attacks[attackIndex].kind != EnemyAttackKind.Body;
                GameObject projectilePrefab = GetOrNull(attackProjectilePrefabs, attackIndex);
                GameObject hitEffectPrefab = GetOrNull(attackHitEffectPrefabs, attackIndex);
                float pitchSpeed = attackPitchSpeeds != null && attackIndex < attackPitchSpeeds.Length ? attackPitchSpeeds[attackIndex] : 0f;
                if (!TryAssignProjectileViews($"{definitionName} 공격 {attackIndex}번", hasProjectile, projectilePrefab, hitEffectPrefab, pitchSpeed, ref attacks[attackIndex].projectile, projectilePrefabs, projectilePitchSpeeds, hitEffectPrefabs))
                {
                    return false;
                }
            }

            return true;
        }

        // 투사체 View 표와 피격 이펙트 표에 프리팹을 올리고(이미 있으면 그 번호를 쓰고) 번호를 투사체 값에 넣는다. 투사체가 없으면 번호는 -1이다.
        private static bool TryAssignProjectileViews(string ownerName, bool hasProjectile, GameObject projectilePrefab, GameObject hitEffectPrefab, float pitchSpeed, ref ProjectileComponent projectile, List<GameObject> projectilePrefabs, List<float> projectilePitchSpeeds, List<GameObject> hitEffectPrefabs)
        {
            projectile.viewIndex = -1;
            projectile.hitEffectIndex = -1;
            if (!hasProjectile)
            {
                return true;
            }

            if (projectilePrefab == null || projectilePrefab.GetComponent<ProjectileView>() == null)
            {
                Debug.LogError($"Runner >> {ownerName}의 투사체 프리팹이 없거나 ProjectileView가 붙어 있지 않아 판을 시작할 수 없다.");
                return false;
            }

            projectile.viewIndex = projectilePrefabs.IndexOf(projectilePrefab);
            if (projectile.viewIndex < 0)
            {
                projectile.viewIndex = projectilePrefabs.Count;
                projectilePrefabs.Add(projectilePrefab);
                projectilePitchSpeeds.Add(pitchSpeed);
            }

            if (hitEffectPrefab == null)
            {
                return true;
            }

            if (hitEffectPrefab.GetComponent<ParticleSystem>() == null)
            {
                Debug.LogError($"Runner >> {ownerName}의 피격 이펙트 프리팹에 ParticleSystem이 없어 판을 시작할 수 없다.");
                return false;
            }

            projectile.hitEffectIndex = hitEffectPrefabs.IndexOf(hitEffectPrefab);
            if (projectile.hitEffectIndex < 0)
            {
                projectile.hitEffectIndex = hitEffectPrefabs.Count;
                hitEffectPrefabs.Add(hitEffectPrefab);
            }

            return true;
        }

        private static GameObject GetOrNull(GameObject[] prefabs, int index)
        {
            return prefabs != null && index < prefabs.Length ? prefabs[index] : null;
        }

        // 프레임 단위 입력을 읽어 플레이어 명령에 쌓는다. 누름 입력은 틱이 소비할 때까지 남는다.
        private void ReadPlayerInput(ref PlayerCommand command)
        {
            PlayerActions.GlobalActions actions = _playerActions.Global;

            Vector2 moveInput = actions.Move.ReadValue<Vector2>();
            command.moveDirection = ToQuarterDirection(new Vector3(moveInput.x, 0f, moveInput.y));
            command.isRunning = actions.Run.IsPressed();
            command.hasAimPoint = TryGetAimPoint(out command.aimPoint);

            if (actions.Jump.WasPerformedThisFrame())
            {
                command.isJumpPressed = true;
            }
            if (actions.Dodge.WasPerformedThisFrame())
            {
                command.isDodgePressed = true;
            }
            if (actions.Attack.WasPerformedThisFrame())
            {
                command.isAttackPressed = true;
            }
            if (actions.Reload.WasPerformedThisFrame())
            {
                command.isReloadPressed = true;
            }
            if (actions.Swap.WasPerformedThisFrame())
            {
                command.swapWeaponIndex = (int)actions.Swap.ReadValue<float>() - 1;
            }
        }

        // 입력 방향을 카메라의 앞·오른쪽(XZ) 기준 월드 방향으로 바꾼다.
        private Vector3 ToQuarterDirection(Vector3 input)
        {
            if (_viewContext.mainCamera == null)
            {
                return input;
            }

            Transform cameraTransform = _viewContext.mainCamera.transform;
            Vector3 cameraForward = cameraTransform.forward;
            Vector3 cameraRight = cameraTransform.right;
            cameraForward.y = 0f;
            cameraRight.y = 0f;
            cameraForward.Normalize();
            cameraRight.Normalize();

            return cameraForward * input.z + cameraRight * input.x;
        }

        // 마우스가 가리키는 점을 총구 높이의 수평면 위에서 구한다. 카메라나 플레이어 View가 없으면 false
        private bool TryGetAimPoint(out Vector3 aimPoint)
        {
            aimPoint = Vector3.zero;
            if (_viewContext.mainCamera == null || _viewContext.playerView == null || Mouse.current == null)
            {
                return false;
            }

            Ray ray = _viewContext.mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
            Plane plane = new Plane(Vector3.up, new Vector3(0f, _viewContext.playerView.muzzle.position.y, 0f));
            if (!plane.Raycast(ray, out float distance))
            {
                return false;
            }

            aimPoint = ray.GetPoint(distance);
            return true;
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

        // Restart와 Exit 명령을 소비한다. 씬과 앱의 전환이라 Tick이 아닌 여기서 처리한다.
        private void HandleSceneCommands(List<GameFlowCommand> gameFlowCommands)
        {
            for (int commandIndex = gameFlowCommands.Count - 1; commandIndex >= 0; commandIndex--)
            {
                GameFlowCommandType type = gameFlowCommands[commandIndex].type;
                if (type == GameFlowCommandType.Restart)
                {
                    gameFlowCommands.RemoveAt(commandIndex);
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
                    SceneManager.LoadScene(SceneManager.GetActiveScene().name);
                    return;
                }

                if (type == GameFlowCommandType.Exit)
                {
                    gameFlowCommands.RemoveAt(commandIndex);
                    Quit();
                    return;
                }
            }
        }

        // 씬의 Spawner Authoring을 이름순으로 읽어 적 종류별 스폰 지점 목록을 만든다. 같은 시드에서 같은 지점이 골라지도록 순서를 고정한다.
        private static SpawnPointComponent[][] CollectEnemySpawnPoints(EnemyDefinition[] enemyDefinitions)
        {
            EnemySpawnerAuthoring[] spawners = FindObjectsByType<EnemySpawnerAuthoring>(FindObjectsSortMode.None);
            SortByName(spawners);

            List<SpawnPointComponent>[] spawnPointLists = new List<SpawnPointComponent>[enemyDefinitions.Length];
            for (int kind = 0; kind < enemyDefinitions.Length; kind++)
            {
                spawnPointLists[kind] = new List<SpawnPointComponent>();
            }

            for (int spawnerIndex = 0; spawnerIndex < spawners.Length; spawnerIndex++)
            {
                EnemySpawnerAuthoring spawner = spawners[spawnerIndex];
                if (spawner.definition == null)
                {
                    Debug.LogWarning($"Runner >> Spawner Authoring {spawner.name}에 Enemy Definition이 없어 건너뛰었다.");
                    continue;
                }

                int kind = System.Array.IndexOf(enemyDefinitions, spawner.definition);
                if (kind < 0)
                {
                    Debug.LogWarning($"Runner >> Spawner Authoring {spawner.name}의 {spawner.definition.name}을 쓰는 웨이브가 없어 건너뛰었다.");
                    continue;
                }

                Transform spawnerTransform = spawner.transform;
                spawnPointLists[kind].Add(new SpawnPointComponent
                {
                    position = spawnerTransform.position,
                    rotation = spawnerTransform.rotation
                });
            }

            SpawnPointComponent[][] spawnPointsByKind = new SpawnPointComponent[enemyDefinitions.Length][];
            for (int kind = 0; kind < enemyDefinitions.Length; kind++)
            {
                if (spawnPointLists[kind].Count == 0)
                {
                    Debug.LogWarning($"Runner >> 씬에 {enemyDefinitions[kind].name}을 가리키는 Spawner Authoring이 없어 그 적이 생기지 않는다.");
                }
                spawnPointsByKind[kind] = spawnPointLists[kind].ToArray();
            }

            return spawnPointsByKind;
        }

        // 씬의 Boss Spawner Authoring을 찾는다. 없거나 둘 이상이면 false
        private static bool TryFindBossSpawner(out BossSpawnerAuthoring bossSpawner)
        {
            BossSpawnerAuthoring[] bossSpawners = FindObjectsByType<BossSpawnerAuthoring>(FindObjectsSortMode.None);
            bossSpawner = null;
            if (bossSpawners.Length != 1)
            {
                Debug.LogError($"Runner >> 씬에 Boss Spawner Authoring이 하나여야 하는데 {bossSpawners.Length}개라 판을 시작할 수 없다.");
                return false;
            }

            bossSpawner = bossSpawners[0];
            return true;
        }

        private static void SortByName(EnemySpawnerAuthoring[] spawners)
        {
            for (int sortedCount = 1; sortedCount < spawners.Length; sortedCount++)
            {
                EnemySpawnerAuthoring current = spawners[sortedCount];
                int insertIndex = sortedCount;
                while (insertIndex > 0 && string.CompareOrdinal(spawners[insertIndex - 1].name, current.name) > 0)
                {
                    spawners[insertIndex] = spawners[insertIndex - 1];
                    insertIndex--;
                }
                spawners[insertIndex] = current;
            }
        }

        private static void Quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
