using TopView.Components;
using TopView.Definitions;
using TopView.Presentation;
using TopView.Presentation.Boss.ViewSystems;
using TopView.Presentation.Camera.ViewSystems;
using TopView.Presentation.Enemy.ViewSystems;
using TopView.Presentation.Player.ViewSystems;
using TopView.Presentation.Projectile.ViewSystems;
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
                PlayerStateSystem.Tick(_world.players, in _world.commandBuffer.playerCommand, _world.playerStateChangedRecords, _world.projectileSpawnRequests, _world.meleeSpawnRequests, _world.melees, _world.gameOverRequests);
                // 막기는 이동이 남긴 이번 틱의 previousPosition을 읽고, 조준은 막힌 뒤의 위치를 읽으므로 이 순서를 지킨다
                PlayerMovementSystem.Tick(_world.players);
                PlayerBlockSystem.Tick(_world.players, _world.enemies, _world.bosses);
                PlayerAimSystem.Tick(_world.players, in _world.commandBuffer.playerCommand);
                PlayerStaminaSystem.Tick(_world.players);
                EnemyBehaviorSystem.Tick(_world.enemies, _world.players, _world.random, _world.enemyStateChangedRecords, _world.projectileSpawnRequests, _world.meleeSpawnRequests);
                BossBehaviorSystem.Tick(_world.bosses, _world.players, _world.random, _world.bossStateChangedRecords, _world.projectileSpawnRequests, _world.meleeSpawnRequests, _world.gameOverRequests);
                EnemyNavigationSystem.Tick(_world.enemies, _world.navigationPath);
                ProjectileSystem.Tick(_world.projectiles);
                HitTargetSystem.Tick(_world.players, _world.enemies, _world.bosses, _world.hitTargets, ref _world.hitTargetCount);
                ProjectileHitSystem.Tick(_world.projectiles, _world.hitTargets, _world.hitTargetCount, _world.damageRequests, _world.projectileHitRecords);
                MeleeHitSystem.Tick(_world.melees, _world.hitTargets, _world.hitTargetCount, _world.damageRequests);
            }

            GameRuleSystem.Tick(ref _world.progress, _world.gameOverRequests);
            if (isPlaying)
            {
                WaveSystem.Tick(in _world.rule, _world.waves, _world.enemySpawnPointsByKind, _world.enemySpawnTemplates, in _world.bossSpawnTemplate, ref _world.progress, _world.random, _world.enemySpawnRequests, _world.bossSpawnRequests);
                PlayerSpawnSystem.Tick(_world.players, _world.playerSpawnRequests);
                EnemySpawnSystem.Tick(_world.enemies, _world.enemySpawnRequests);
                BossSpawnSystem.Tick(_world.bosses, _world.bossSpawnRequests, _world.players);
                ProjectileSpawnSystem.Tick(_world.projectiles, _world.projectileSpawnRequests);
                MeleeSpawnSystem.Tick(_world.melees, _world.meleeSpawnRequests);
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

            PlayerViewSystem.Present(_world.players, _world.playerStateChangedRecords, _playerViewPrefab, _playerWeaponModelPrefabs, _playerUpperLayerBlendInTicks, _viewContext);
            EnemyViewSystem.Present(_world.enemies, _world.enemyStateChangedRecords, _enemyViewPrefabs, _viewContext);
            EnemyHealthGaugeViewSystem.Present(_world.enemies, _enemyHealthGaugePrefabs, _enemyHealthGaugeOffsets, _enemyHealthGaugeColors, _viewContext);
            BossViewSystem.Present(_world.bosses, _world.bossStateChangedRecords, _bossViewPrefab, _viewContext);
            ProjectileViewSystem.Present(_world.projectiles, _projectilePrefabs, _projectilePitchSpeeds, _viewContext);
            HitEffectViewSystem.Present(_world.projectileHitRecords, _hitEffectPrefabs, _viewContext);
            CameraViewSystem.Present(_viewContext, _playerCameraPriority);

            PresentUserInterface();

            _world.playerStateChangedRecords.Clear();
            _world.enemyStateChangedRecords.Clear();
            _world.bossStateChangedRecords.Clear();
            _world.projectileHitRecords.Clear();
        }
    }
}
