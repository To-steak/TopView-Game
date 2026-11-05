using System.Collections.Generic;
using TopView.Authoring;
using TopView.Components;
using TopView.Definitions;
using TopView.Presentation.Views;
using TopView.Requests;
using UnityEngine;

namespace TopView.Core
{
    // 적: 스포너의 적 종류와 스폰 지점, Definition 읽기
    public partial class Runner
    {
        // 적 종류별 View 프리팹. 번호가 적 종류 번호다
        private GameObject[] _enemyViewPrefabs;
        // 적 종류별 체력 게이지 프리팹. 번호가 적 종류 번호다
        private GameObject[] _enemyHealthGaugePrefabs;

        // 씬의 켜진 Spawner Authoring을 이름순으로 읽어 적 종류 목록과 스폰 지점 목록을 만든다. 적 종류 번호는 스포너 이름순에서 처음 나온 순서다. 같은 시드에서 같은 결과가 나오도록 순서를 고정한다.
        // spawnerAuthorings는 스폰 지점과 번호가 같은 Authoring 목록이다. 감지기의 연결을 번호로 바꿀 때 쓴다.
        private static void ReadEnemySpawners(out EnemyDefinition[] enemyDefinitions, out SpawnPointComponent[] spawnPoints, out EnemySpawnerAuthoring[] spawnerAuthorings)
        {
            EnemySpawnerAuthoring[] spawners = FindObjectsByType<EnemySpawnerAuthoring>(FindObjectsSortMode.None);
            SortByName(spawners);

            List<EnemyDefinition> kinds = new List<EnemyDefinition>();
            List<SpawnPointComponent> spawnPointList = new List<SpawnPointComponent>();
            List<EnemySpawnerAuthoring> spawnerAuthoringList = new List<EnemySpawnerAuthoring>();
            for (int spawnerIndex = 0; spawnerIndex < spawners.Length; spawnerIndex++)
            {
                EnemySpawnerAuthoring spawner = spawners[spawnerIndex];
                if (spawner.definition == null)
                {
                    Debug.LogWarning($"Runner >> Spawner Authoring {spawner.name}에 Enemy Definition이 없어 건너뛰었다.");
                    continue;
                }

                int kind = kinds.IndexOf(spawner.definition);
                if (kind < 0)
                {
                    kind = kinds.Count;
                    kinds.Add(spawner.definition);
                }

                Transform spawnerTransform = spawner.transform;
                spawnPointList.Add(new SpawnPointComponent
                {
                    enemyKindIndex = kind,
                    position = spawnerTransform.position,
                    rotation = spawnerTransform.rotation
                });
                spawnerAuthoringList.Add(spawner);
            }

            if (spawnPointList.Count == 0)
            {
                Debug.LogWarning("Runner >> 씬에 켜진 Enemy Spawner Authoring이 없어 적이 생기지 않는다.");
            }

            enemyDefinitions = kinds.ToArray();
            spawnPoints = spawnPointList.ToArray();
            spawnerAuthorings = spawnerAuthoringList.ToArray();
        }

        // 씬의 켜진 Player Detector Authoring을 이름순으로 읽어 감지기 목록을 만든다. 연결된 스포너는 spawnerAuthorings에서의 번호로 바꾼다.
        private static PlayerDetectorComponent[] ReadPlayerDetectors(EnemySpawnerAuthoring[] spawnerAuthorings)
        {
            PlayerDetectorAuthoring[] authorings = FindObjectsByType<PlayerDetectorAuthoring>(FindObjectsSortMode.None);
            SortByName(authorings);

            List<PlayerDetectorComponent> detectors = new List<PlayerDetectorComponent>();
            for (int authoringIndex = 0; authoringIndex < authorings.Length; authoringIndex++)
            {
                PlayerDetectorAuthoring authoring = authorings[authoringIndex];
                if (authoring.range.detectionRadius <= 0f)
                {
                    Debug.LogWarning($"Runner >> Player Detector Authoring {authoring.name}의 Detection Radius가 0 이하라 건너뛰었다.");
                    continue;
                }

                int[] spawnerIndices = ReadLinkedSpawnerIndices(authoring, spawnerAuthorings);
                if (spawnerIndices.Length == 0)
                {
                    Debug.LogWarning($"Runner >> Player Detector Authoring {authoring.name}에 발동할 수 있는 스포너가 없어 건너뛰었다.");
                    continue;
                }

                detectors.Add(new PlayerDetectorComponent
                {
                    position = authoring.transform.position,
                    range = authoring.range,
                    spawnerIndices = spawnerIndices
                });
            }

            return detectors.ToArray();
        }

        // 감지기에 연결된 스포너를 번호로 바꾼다. 빈 슬롯과 판에서 쓰지 않는 스포너(꺼졌거나 Definition이 없음)는 경고를 남기고 뺀다.
        private static int[] ReadLinkedSpawnerIndices(PlayerDetectorAuthoring authoring, EnemySpawnerAuthoring[] spawnerAuthorings)
        {
            if (authoring.spawners == null)
            {
                return new int[0];
            }

            List<int> spawnerIndices = new List<int>();
            for (int linkIndex = 0; linkIndex < authoring.spawners.Length; linkIndex++)
            {
                EnemySpawnerAuthoring spawner = authoring.spawners[linkIndex];
                if (spawner == null)
                {
                    Debug.LogWarning($"Runner >> Player Detector Authoring {authoring.name}의 Spawners {linkIndex}번 슬롯이 비어 있어 건너뛰었다.");
                    continue;
                }

                int spawnerIndex = System.Array.IndexOf(spawnerAuthorings, spawner);
                if (spawnerIndex < 0)
                {
                    Debug.LogWarning($"Runner >> Player Detector Authoring {authoring.name}에 연결된 스포너 {spawner.name}가 꺼져 있거나 Enemy Definition이 없어 건너뛰었다.");
                    continue;
                }

                spawnerIndices.Add(spawnerIndex);
            }

            return spawnerIndices.ToArray();
        }

        // 적 종류별 Enemy Definition에서 생성 요청 템플릿과 View·게이지 값을 꺼낸다.
        private static void ReadEnemies(EnemyDefinition[] enemyDefinitions, List<GameObject> projectilePrefabs, List<GameObject> hitEffectPrefabs, out EnemySpawnRequest[] spawnTemplates, out GameObject[] viewPrefabs, out GameObject[] gaugePrefabs)
        {
            int enemyKindCount = enemyDefinitions.Length;
            spawnTemplates = new EnemySpawnRequest[enemyKindCount];
            viewPrefabs = new GameObject[enemyKindCount];
            gaugePrefabs = new GameObject[enemyKindCount];

            for (int kind = 0; kind < enemyKindCount; kind++)
            {
                EnemyDefinition definition = enemyDefinitions[kind];
                EnemyAttackComponent[] attacks = ReadAttacks(definition.name, definition.attacks, projectilePrefabs, hitEffectPrefabs);
                WarnIfStopsOutOfRange(definition.name, definition.behavior.stopDistance, attacks);

                spawnTemplates[kind] = new EnemySpawnRequest
                {
                    enemyKindIndex = kind,
                    stats = definition.stats,
                    behavior = definition.behavior,
                    attacks = attacks,
                    loots = ReadLoots(definition.name, definition.loots)
                };
                // View는 표시용이라 빠져 있어도 판은 시작하고, 그 종류는 보이지 않게 둔다
                EnemyView enemyView = definition.viewPrefab != null ? definition.viewPrefab.GetComponent<EnemyView>() : null;
                if (enemyView == null)
                {
                    Debug.LogWarning($"Runner >> {definition.name}의 View Prefab이 없거나 EnemyView가 붙어 있지 않아 이 적 종류는 보이지 않게 진행한다.");
                }
                else
                {
                    viewPrefabs[kind] = definition.viewPrefab;
                    if (enemyView.damageAnchor == null)
                    {
                        Debug.LogWarning($"Runner >> {definition.name}의 View Prefab에 Damage Anchor가 연결되어 있지 않아 이 적 종류의 피해량 글자는 적의 발밑에 뜬다.");
                    }
                }
                // 오버레이 체력 바로 체력을 보여 주는 종류는 게이지를 두지 않아도 된다
                if (definition.healthGaugePrefab == null && definition.behavior.showsOverlayHealthBar)
                {
                    continue;
                }
                // 체력 게이지는 표시용이라 빠져 있어도 판은 시작하고, 그 종류는 게이지 없이 둔다
                if (definition.healthGaugePrefab == null || definition.healthGaugePrefab.GetComponent<EnemyHealthGaugeView>() == null)
                {
                    Debug.LogWarning($"Runner >> {definition.name}의 Health Gauge Prefab이 없거나 EnemyHealthGaugeView가 붙어 있지 않아 이 적 종류는 체력 게이지 없이 진행한다.");
                }
                else if (enemyView != null && enemyView.healthGaugeAnchor == null)
                {
                    Debug.LogWarning($"Runner >> {definition.name}의 View Prefab에 Health Gauge Anchor가 연결되어 있지 않아 이 적 종류는 체력 게이지 없이 진행한다.");
                }
                else
                {
                    gaugePrefabs[kind] = definition.healthGaugePrefab;
                }
            }
        }

        // 멈추는 거리가 모든 공격의 사거리보다 크면 멈춘 자리에서 공격하지 못하므로 경고를 남긴다.
        private static void WarnIfStopsOutOfRange(string definitionName, float stopDistance, EnemyAttackComponent[] attacks)
        {
            if (attacks.Length == 0)
            {
                return;
            }

            float maxAttackRange = 0f;
            for (int attackIndex = 0; attackIndex < attacks.Length; attackIndex++)
            {
                maxAttackRange = Mathf.Max(maxAttackRange, attacks[attackIndex].attackRange);
            }

            if (stopDistance > maxAttackRange)
            {
                Debug.LogWarning($"Runner >> {definitionName}의 Stop Distance({stopDistance})가 가장 긴 Attack Range({maxAttackRange})보다 커서 멈춘 자리에서 공격하지 못할 수 있다.");
            }
        }

        private static void SortByName<T>(T[] authorings) where T : MonoBehaviour
        {
            for (int sortedCount = 1; sortedCount < authorings.Length; sortedCount++)
            {
                T current = authorings[sortedCount];
                int insertIndex = sortedCount;
                while (insertIndex > 0 && string.CompareOrdinal(authorings[insertIndex - 1].name, current.name) > 0)
                {
                    authorings[insertIndex] = authorings[insertIndex - 1];
                    insertIndex--;
                }
                authorings[insertIndex] = current;
            }
        }
    }
}
