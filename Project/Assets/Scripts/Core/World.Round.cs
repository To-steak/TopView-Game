using System.Collections.Generic;
using TopView.Authoring;
using TopView.Components;
using TopView.Definitions;
using TopView.Entities;
using TopView.Presentation.Views;
using TopView.Presentation.ViewContexts;
using TopView.Presentation.ViewSystems;
using TopView.Requests;
using UnityEngine;
using UnityEngine.AI;

namespace TopView.Core
{
    // 판 준비: 한 판의 데이터와 View 실행 상태를 만들고, 판을 끝낼 때 비운다
    public partial class World
    {
        // 화면을 그리는 메인 카메라. 조준점, 이동 방향, 월드 UI가 기준으로 쓴다
        [SerializeField] private Camera _mainCamera;
        // 플레이어를 따라가는 시네머신 카메라
        [SerializeField] private PlayerCameraView _playerCameraView;
        // 적 체력 게이지, 월드 글자, 월드 정보 UI를 담는 월드 캔버스
        [SerializeField] private WorldCanvasView _worldCanvasView;

        // 적 종류마다 동시에 있을 수 있는 최대 수. 엔티티 저장소의 적 종류별 구간 칸 수다
        private const int ENEMY_CAPACITY_PER_KIND = 512;
        // 적 하나의 경로에 담을 수 있는 꺾이는 점의 최대 수. 시작점도 한 칸을 쓴다
        private const int ENEMY_PATH_CORNER_CAPACITY = 8;
        // 동시에 있을 수 있는 직선 투사체 최대 수. 엔티티 저장소의 직선 투사체 구간 칸 수다
        private const int STRAIGHT_PROJECTILE_CAPACITY = 512;
        // 동시에 있을 수 있는 포물선 투사체 최대 수. 엔티티 저장소의 포물선 투사체 구간 칸 수다
        private const int ARC_PROJECTILE_CAPACITY = 64;
        // 동시에 있을 수 있는 전리품 최대 수. 엔티티 저장소의 전리품 구간 칸 수다
        private const int LOOT_CAPACITY = 512;

        // 한 판의 데이터에 Definition과 씬의 Authoring 값을 채운 뒤, 씬의 View와 Presenter를 연결한다.
        private void StartRound()
        {
            if (!TryFindPlayerSpawner(out PlayerSpawnerAuthoring playerSpawner))
            {
                return;
            }

            GameSettingsDefinition gameSettings = _gameSettingsDefinition;
            if (gameSettings == null)
            {
                Debug.LogError("World >> World에 Game Settings Definition이 연결되어 있지 않아 판을 시작할 수 없다.");
                return;
            }

            LootDefinition lootDefinition = _lootDefinition;
            if (lootDefinition == null)
            {
                Debug.LogError("World >> World에 Loot Definition이 연결되어 있지 않아 판을 시작할 수 없다.");
                return;
            }

            List<GameObject> projectilePrefabs = new List<GameObject>();
            List<GameObject> hitEffectPrefabs = new List<GameObject>();

            if (!TryReadPlayer(playerSpawner, projectilePrefabs, hitEffectPrefabs, out PlayerDefinition playerDefinition, out PlayerSpawnRequest playerSpawnRequest, out GameObject[] weaponModelPrefabs))
            {
                return;
            }

            if (lootDefinition.rule.pickupRadius <= 0f || lootDefinition.rule.lifetimeTicks <= 0)
            {
                Debug.LogError("World >> Loot Definition의 Pickup Radius나 Lifetime Ticks가 0 이하라 판을 시작할 수 없다.");
                return;
            }

            if (gameSettings.rule.navMeshSampleDistance <= 0f)
            {
                Debug.LogError("World >> Game Settings Definition의 Nav Mesh Sample Distance가 0 이하라 판을 시작할 수 없다.");
                return;
            }

            if (gameSettings.rule.navigationRetryIntervalTicks <= 0)
            {
                Debug.LogWarning("World >> Game Settings Definition의 Navigation Retry Interval Ticks가 0 이하라, 경로를 구하지 못한 적이 기다리지 않고 바로 다시 길을 찾는다. 적이 Idle과 Walk를 매 틱 오갈 수 있다.");
            }

            if (gameSettings.rule.unstuckSampleDistance <= 0f)
            {
                Debug.LogError("World >> Game Settings Definition의 Unstuck Sample Distance가 0 이하라 판을 시작할 수 없다.");
                return;
            }

            if (gameSettings.rule.enemyLevelUpIntervalTicks <= 0)
            {
                Debug.LogError("World >> Game Settings Definition의 Enemy Level Up Interval Ticks가 0 이하라 판을 시작할 수 없다.");
                return;
            }

            ReadEnemySpawners(out EnemyDefinition[] enemyDefinitions, out EnemySpawnerSpawnRequest[] enemySpawnerSpawnRequests, out EnemySpawnerAuthoring[] enemySpawnerAuthorings);
            PlayerDetectorSpawnRequest[] playerDetectorSpawnRequests = ReadPlayerDetectors(enemySpawnerAuthorings);

            int enemyKindCount = enemyDefinitions.Length;
            ReadEnemies(enemyDefinitions, projectilePrefabs, hitEffectPrefabs, out EnemySpawnRequest[] enemySpawnTemplates, out GameObject[] enemyViewPrefabs, out EnemyAttackAnimation[][] enemyAttackAnimations, out AudioClip[][] enemyAttackClips, out bool[] enemyHasOverlayHealthBar, out GameObject[] enemyHealthGaugePrefabs);
            WarnMissingBossClip(_musicDefinition, enemyHasOverlayHealthBar);

            _rule = gameSettings.rule;
            _lootRule = lootDefinition.rule;
            _random = new System.Random(_rule.randomSeed);
            _playerSpawnRequests.Enqueue(playerSpawnRequest);
            for (int spawnerIndex = 0; spawnerIndex < enemySpawnerSpawnRequests.Length; spawnerIndex++)
            {
                _enemySpawnerSpawnRequests.Enqueue(enemySpawnerSpawnRequests[spawnerIndex]);
            }
            for (int detectorIndex = 0; detectorIndex < playerDetectorSpawnRequests.Length; detectorIndex++)
            {
                _playerDetectorSpawnRequests.Enqueue(playerDetectorSpawnRequests[detectorIndex]);
            }
            _enemySpawnTemplates = enemySpawnTemplates;
            int enemyCapacity = enemyKindCount * ENEMY_CAPACITY_PER_KIND;
            _playerHitTargets = new HitTargetComponent[PLAYER_CAPACITY];
            _enemyHitTargets = new HitTargetComponent[enemyCapacity];
            // 공격하는 쪽 하나는 근접 판정을 동시에 하나만 가지고, 한 판정이 맞힐 대상도 플레이어와 적의 칸 수를 넘지 않는다
            int combatantCapacity = PLAYER_CAPACITY + enemyCapacity;
            _entities = new EntityStorage(LOOT_CAPACITY, combatantCapacity, combatantCapacity, STRAIGHT_PROJECTILE_CAPACITY, ARC_PROJECTILE_CAPACITY, enemyKindCount, ENEMY_CAPACITY_PER_KIND, ENEMY_PATH_CORNER_CAPACITY, PLAYER_CAPACITY, PLAYER_CAPACITY * playerSpawnRequest.weapons.Length,
                enemySpawnerSpawnRequests.Length, playerDetectorSpawnRequests.Length);

            _progress = new GameProgressComponent
            {
                phase = GamePhase.Playing,
                remainingTicks = _rule.timeLimitTicks,
                score = 0,
                gold = 0,
                enemyLevel = _rule.startingEnemyLevel,
                enemyLevelUpRemainingTicks = _rule.enemyLevelUpIntervalTicks
            };

            _navigationPath = new NavMeshPath();

            _isRoundReady = true;
            _playerViewPrefab = playerDefinition.viewPrefab;
            _playerCameraPriority = playerDefinition.cameraPriority;
            _playerCameraYawDegreesPerSecond = playerDefinition.cameraYawDegreesPerSecond;
            _playerRangedLayerBlendInTicks = playerDefinition.rangedLayerBlendInTicks;
            _playerThrowPreviewCircleHeightOffset = playerDefinition.throwPreviewCircleHeightOffset;
            _playerWeaponModelPrefabs = weaponModelPrefabs;
            _enemyViewPrefabs = enemyViewPrefabs;
            _enemyAttackAnimations = enemyAttackAnimations;
            _enemyAttackClips = enemyAttackClips;
            _enemyHasOverlayHealthBar = enemyHasOverlayHealthBar;
            _enemyHealthGaugePrefabs = enemyHealthGaugePrefabs;
            _projectilePrefabs = projectilePrefabs.ToArray();
            _hitEffectPrefabs = hitEffectPrefabs.ToArray();
            _lootPrefabs = ReadLootPrefabs(lootDefinition);
            ReadWorldText(_worldTextPrefab);
            _viewContext = CreateViewContext(enemyKindCount);

            BindPresenters();
        }

        // 판을 멈추고 View 실행 상태를 비운다. 씬을 내리며 OnDestroy에서 부른다. 씬이 바뀌기까지 남은 프레임에 판이 돌지 않게 하는 일은 App이 World 등록을 먼저 풀어서 한다.
        private void ClearRound()
        {
            if (_viewContext != null)
            {
                PauseViewSystem.Release(_viewContext.pause);
                CursorViewSystem.Release(_viewContext.cursor);
            }

            _isRoundReady = false;
            _viewContext = null;
        }

        // 이번 판의 저장소 크기와 View 표에 맞춰 View 실행 상태를 만든다.
        private ViewContext CreateViewContext(int enemyKindCount)
        {
            WorldInformationAnchorView[] worldInformationAnchors = ReadWorldInformationAnchors();
            ViewContext viewContext = new ViewContext
            {
                camera = new CameraViewContext
                {
                    mainCamera = _mainCamera,
                    playerCameraView = _playerCameraView
                },
                player = new PlayerViewContext(),
                enemy = new EnemyViewContext
                {
                    views = new EnemyView[_entities.entities.Length],
                    viewGenerations = new int[_entities.entities.Length],
                    viewPools = new Stack<EnemyView>[enemyKindCount],
                    triggerHashesByKind = new int[enemyKindCount][],
                    healthGaugeViews = new EnemyHealthGaugeView[_entities.entities.Length],
                    healthGaugeGenerations = new int[_entities.entities.Length],
                    healthGaugeRatios = new float[_entities.entities.Length],
                    healthGaugePools = new Stack<EnemyHealthGaugeView>[enemyKindCount]
                },
                projectile = CreateProjectileViewContext(_entities.entities.Length),
                hitEffect = new HitEffectViewContext
                {
                    pools = new Stack<ParticleSystem>[_hitEffectPrefabs.Length]
                },
                worldText = new WorldTextViewContext(),
                worldInformation = new WorldInformationViewContext
                {
                    anchors = worldInformationAnchors,
                    views = new WorldInformationView[worldInformationAnchors.Length]
                },
                loot = new LootViewContext
                {
                    views = new LootView[_entities.entities.Length],
                    viewGenerations = new int[_entities.entities.Length],
                    viewKinds = new int[_entities.entities.Length],
                    viewPools = new Stack<LootView>[_lootPrefabs.Length]
                },
                pause = new PauseViewContext(),
                cursor = new CursorViewContext()
            };
            for (int kind = 0; kind < enemyKindCount; kind++)
            {
                viewContext.enemy.viewPools[kind] = new Stack<EnemyView>();
                viewContext.enemy.healthGaugePools[kind] = new Stack<EnemyHealthGaugeView>();
            }
            for (int effectIndex = 0; effectIndex < _hitEffectPrefabs.Length; effectIndex++)
            {
                viewContext.hitEffect.pools[effectIndex] = new Stack<ParticleSystem>();
            }
            for (int lootKind = 0; lootKind < _lootPrefabs.Length; lootKind++)
            {
                viewContext.loot.viewPools[lootKind] = new Stack<LootView>();
            }
            if (viewContext.camera.mainCamera == null)
            {
                Debug.LogError("World >> Main Camera가 연결되어 있지 않아 조준과 카메라 기준 이동, 월드 UI가 동작하지 않는다.");
            }
            if (viewContext.camera.playerCameraView == null)
            {
                Debug.LogError("World >> Player Camera View가 연결되어 있지 않다.");
            }
            if (_gameSettingsDefinition.crosshairCursor == null)
            {
                Debug.LogError("World >> Game Settings Definition에 Crosshair Cursor가 연결되어 있지 않아 플레이 중에도 기본 커서를 쓴다.");
            }
            if (_worldCanvasView == null)
            {
                Debug.LogError("World >> World Canvas View가 연결되어 있지 않아 적 체력 게이지, 월드 글자, 월드 정보 UI가 보이지 않는다.");
            }
            else
            {
                Transform worldCanvasTransform = _worldCanvasView.transform;
                viewContext.enemy.healthGaugeParent = worldCanvasTransform;
                viewContext.worldText.parent = worldCanvasTransform;
                viewContext.worldInformation.parent = worldCanvasTransform;
            }

            return viewContext;
        }

        // 엔티티 저장소의 칸 수와 투사체 View 표에 맞춰 투사체 View 실행 상태를 만든다.
        private ProjectileViewContext CreateProjectileViewContext(int capacity)
        {
            ProjectileViewContext context = new ProjectileViewContext
            {
                views = new ProjectileView[capacity],
                viewGenerations = new int[capacity],
                viewIndices = new int[capacity],
                viewPools = new Stack<ProjectileView>[_projectilePrefabs.Length]
            };
            for (int viewIndex = 0; viewIndex < _projectilePrefabs.Length; viewIndex++)
            {
                context.viewPools[viewIndex] = new Stack<ProjectileView>();
            }

            return context;
        }

        // 보스 종류(오버레이 체력 바를 보여 주는 적)가 있는데 Music Definition에 보스 곡이 없으면 알린다. 보스가 나와도 평소 곡을 계속 튼다.
        // Music Definition이 없으면 App이 씬을 등록할 때 이미 알렸으므로 넘어간다.
        private static void WarnMissingBossClip(MusicDefinition musicDefinition, bool[] enemyHasOverlayHealthBar)
        {
            if (musicDefinition == null || musicDefinition.bossClip != null)
            {
                return;
            }

            foreach (bool isBossKind in enemyHasOverlayHealthBar)
            {
                if (isBossKind)
                {
                    Debug.LogWarning("World >> 보스 적 종류가 있는데 Music Definition에 Boss Clip이 연결되어 있지 않아 보스가 나와도 평소 곡을 계속 튼다.");
                    return;
                }
            }
        }
    }
}
