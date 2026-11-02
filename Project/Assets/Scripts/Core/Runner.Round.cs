using System.Collections.Generic;
using TopView.Authoring;
using TopView.Components;
using TopView.Definitions;
using TopView.Entities;
using TopView.Presentation;
using TopView.Presentation.Camera.Views;
using TopView.Presentation.Enemy.Views;
using TopView.Presentation.Projectile.Views;
using TopView.Presentation.WorldCanvas.Views;
using TopView.Requests;
using UnityEngine;

namespace TopView.Core
{
    // 판 준비: World와 View 실행 상태를 만들고, 판을 끝낼 때 비운다
    public partial class Runner
    {
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
            if (!TryReadEnemies(enemyDefinitions, projectilePrefabs, projectilePitchSpeeds, hitEffectPrefabs, out EnemySpawnRequest[] enemySpawnTemplates, out GameObject[] enemyViewPrefabs, out GameObject[] enemyHealthGaugePrefabs, out Vector3[] enemyHealthGaugeOffsets, out Color[] enemyHealthGaugeColors))
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
            // 피격 대상은 세 저장소의 칸 수를 넘지 않고, 공격하는 쪽 하나는 근접 판정을 동시에 하나만 가지므로 근접 판정도 이 수로 넘치지 않는다
            int combatantCapacity = world.players.entities.Length + world.enemies.entities.Length + world.bosses.entities.Length;
            world.hitTargets = new HitTargetComponent[combatantCapacity];
            world.melees = new MeleeStorage(combatantCapacity, combatantCapacity);

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
            _playerRangedLayerBlendInTicks = playerDefinition.rangedLayerBlendInTicks;
            _playerWeaponModelPrefabs = weaponModelPrefabs;
            _enemyViewPrefabs = enemyViewPrefabs;
            _enemyHealthGaugePrefabs = enemyHealthGaugePrefabs;
            _enemyHealthGaugeOffsets = enemyHealthGaugeOffsets;
            _enemyHealthGaugeColors = enemyHealthGaugeColors;
            _bossViewPrefab = bossDefinition.viewPrefab;
            _projectilePrefabs = projectilePrefabs.ToArray();
            _projectilePitchSpeeds = projectilePitchSpeeds.ToArray();
            _hitEffectPrefabs = hitEffectPrefabs.ToArray();
            _viewContext = CreateViewContext(enemyKindCount);

            BindPresenters();
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
            _playerManaPresenter = null;
            _bossHealthPresenter = null;
        }

        // 이번 판의 저장소 크기와 View 표에 맞춰 View 실행 상태를 만들고 씬의 카메라를 찾는다.
        private ViewContext CreateViewContext(int enemyKindCount)
        {
            ViewContext viewContext = new ViewContext
            {
                camera = new CameraViewContext
                {
                    mainCamera = UnityEngine.Camera.main,
                    playerCameraView = FindAnyObjectByType<PlayerCameraView>()
                },
                player = new PlayerViewContext(),
                enemy = new EnemyViewContext
                {
                    views = new EnemyView[_world.enemies.entities.Length],
                    viewGenerations = new int[_world.enemies.entities.Length],
                    viewPools = new Stack<EnemyView>[enemyKindCount],
                    triggerHashesByKind = new int[enemyKindCount][],
                    healthGaugeViews = new EnemyHealthGaugeView[_world.enemies.entities.Length],
                    healthGaugePools = new Stack<EnemyHealthGaugeView>[enemyKindCount]
                },
                boss = new BossViewContext(),
                projectile = new ProjectileViewContext
                {
                    views = new ProjectileView[_world.projectiles.entities.Length],
                    viewGenerations = new int[_world.projectiles.entities.Length],
                    viewIndices = new int[_world.projectiles.entities.Length],
                    viewPools = new Stack<ProjectileView>[_projectilePrefabs.Length]
                },
                hitEffect = new HitEffectViewContext
                {
                    pools = new Stack<ParticleSystem>[_hitEffectPrefabs.Length]
                }
            };
            for (int kind = 0; kind < enemyKindCount; kind++)
            {
                viewContext.enemy.viewPools[kind] = new Stack<EnemyView>();
                viewContext.enemy.healthGaugePools[kind] = new Stack<EnemyHealthGaugeView>();
            }
            for (int viewIndex = 0; viewIndex < _projectilePrefabs.Length; viewIndex++)
            {
                viewContext.projectile.viewPools[viewIndex] = new Stack<ProjectileView>();
            }
            for (int effectIndex = 0; effectIndex < _hitEffectPrefabs.Length; effectIndex++)
            {
                viewContext.hitEffect.pools[effectIndex] = new Stack<ParticleSystem>();
            }
            if (viewContext.camera.mainCamera == null)
            {
                Debug.LogError("Runner >> 씬에 MainCamera 태그를 가진 카메라가 없다.");
            }
            if (viewContext.camera.playerCameraView == null)
            {
                Debug.LogError("Runner >> 씬에 PlayerCameraView가 없다.");
            }
            WorldCanvasView worldCanvasView = FindAnyObjectByType<WorldCanvasView>();
            if (worldCanvasView == null)
            {
                Debug.LogError("Runner >> 씬에 WorldCanvasView가 없어 적 체력 게이지가 보이지 않는다.");
            }
            else
            {
                viewContext.enemy.healthGaugeParent = worldCanvasView.transform;
            }

            return viewContext;
        }
    }
}
