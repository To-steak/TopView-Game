using System.Collections.Generic;
using TopView.Authoring;
using TopView.Components;
using TopView.Definitions;
using TopView.Presentation.Views;
using TopView.Presentation.ViewSystems;
using TopView.Requests;
using UnityEngine;

namespace TopView.Core
{
    // 적: 스포너와 플레이어 감지기, Definition 읽기
    public partial class Runner
    {
        // 적 종류별 View 프리팹. 번호가 적 종류 번호다
        private GameObject[] _enemyViewPrefabs;
        // 적 종류별, 공격 번호별 애니메이션과 공격 소리. 판을 준비할 때 Definition에서 모은다
        private EnemyAttackAnimation[][] _enemyAttackAnimations;
        private AudioClip[][] _enemyAttackClips;
        // 적 종류별 체력 게이지 프리팹. 번호가 적 종류 번호다
        private GameObject[] _enemyHealthGaugePrefabs;

        // 씬의 켜진 Spawner Authoring을 이름순으로 읽어 적 종류 목록과 스포너 목록을 만든다. 적 종류 번호는 스포너 이름순에서 처음 나온 순서다. 같은 시드에서 같은 결과가 나오도록 순서를 고정한다.
        // spawnerAuthorings는 스포너와 번호가 같은 Authoring 목록이다. 감지기의 연결을 번호로 바꿀 때 쓴다.
        private static void ReadEnemySpawners(out EnemyDefinition[] enemyDefinitions, out EnemySpawnerComponent[] enemySpawners, out EnemySpawnerAuthoring[] spawnerAuthorings)
        {
            EnemySpawnerAuthoring[] spawners = FindObjectsByType<EnemySpawnerAuthoring>(FindObjectsSortMode.None);
            SortByName(spawners);

            List<EnemyDefinition> kinds = new List<EnemyDefinition>();
            List<EnemySpawnerComponent> enemySpawnerList = new List<EnemySpawnerComponent>();
            List<EnemySpawnerAuthoring> spawnerAuthoringList = new List<EnemySpawnerAuthoring>();
            for (int spawnerIndex = 0; spawnerIndex < spawners.Length; spawnerIndex++)
            {
                EnemySpawnerAuthoring spawner = spawners[spawnerIndex];
                if (spawner.definition == null)
                {
                    Debug.LogWarning($"Runner >> Spawner Authoring {spawner.name}에 Enemy Definition이 없어 건너뛰었다.");
                    continue;
                }

                if (spawner.rule.spawnIntervalTicks <= 0)
                {
                    Debug.LogWarning($"Runner >> Spawner Authoring {spawner.name}의 Spawn Interval Ticks가 0 이하라 건너뛰었다.");
                    continue;
                }

                if (!spawner.rule.isInfinite && spawner.rule.maxSpawnCount <= 0)
                {
                    Debug.LogWarning($"Runner >> Spawner Authoring {spawner.name}의 Max Spawn Count가 0 이하라 건너뛰었다.");
                    continue;
                }

                int kind = kinds.IndexOf(spawner.definition);
                if (kind < 0)
                {
                    kind = kinds.Count;
                    kinds.Add(spawner.definition);
                }

                Transform spawnerTransform = spawner.transform;
                enemySpawnerList.Add(new EnemySpawnerComponent
                {
                    enemyKindIndex = kind,
                    position = spawnerTransform.position,
                    rotation = spawnerTransform.rotation,
                    rule = spawner.rule
                });
                spawnerAuthoringList.Add(spawner);
            }

            if (enemySpawnerList.Count == 0)
            {
                Debug.LogWarning("Runner >> 씬에 켜진 Enemy Spawner Authoring이 없어 적이 생기지 않는다.");
            }

            enemyDefinitions = kinds.ToArray();
            enemySpawners = enemySpawnerList.ToArray();
            spawnerAuthorings = spawnerAuthoringList.ToArray();
        }

        // 씬의 켜진 Player Detector Authoring을 이름순으로 읽어 감지기 목록을 만든다. 연결된 스포너는 spawnerAuthorings에서의 번호로 바꾼다.
        private static PlayerDetectorComponent[] ReadPlayerDetectors(EnemySpawnerAuthoring[] spawnerAuthorings)
        {
            PlayerDetectorAuthoring[] authorings = FindObjectsByType<PlayerDetectorAuthoring>(FindObjectsSortMode.None);
            SortByName(authorings);

            List<PlayerDetectorComponent> detectors = new List<PlayerDetectorComponent>();
            bool[] isSpawnerLinked = new bool[spawnerAuthorings.Length];
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
                for (int linkIndex = 0; linkIndex < spawnerIndices.Length; linkIndex++)
                {
                    isSpawnerLinked[spawnerIndices[linkIndex]] = true;
                }
            }

            for (int spawnerIndex = 0; spawnerIndex < spawnerAuthorings.Length; spawnerIndex++)
            {
                if (!isSpawnerLinked[spawnerIndex])
                {
                    Debug.LogWarning($"Runner >> Spawner Authoring {spawnerAuthorings[spawnerIndex].name}가 어느 Player Detector Authoring에도 연결되어 있지 않아 적을 만들지 않는다.");
                }
            }

            return detectors.ToArray();
        }

        // 감지기에 연결된 스포너를 번호로 바꾼다. 빈 슬롯과 판에서 쓰지 않는 스포너(꺼졌거나 설정이 잘못됨)는 경고를 남기고 뺀다.
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
                    Debug.LogWarning($"Runner >> Player Detector Authoring {authoring.name}에 연결된 스포너 {spawner.name}가 꺼져 있거나 설정이 잘못되어 판에서 빠졌으므로 건너뛰었다.");
                    continue;
                }

                spawnerIndices.Add(spawnerIndex);
            }

            return spawnerIndices.ToArray();
        }

        // 적 종류별 Enemy Definition에서 생성 요청 템플릿과 View·게이지 값을 꺼낸다.
        private static void ReadEnemies(EnemyDefinition[] enemyDefinitions, List<GameObject> projectilePrefabs, List<GameObject> hitEffectPrefabs, out EnemySpawnRequest[] spawnTemplates, out GameObject[] viewPrefabs, out EnemyAttackAnimation[][] attackAnimations, out AudioClip[][] attackClips, out GameObject[] gaugePrefabs)
        {
            int enemyKindCount = enemyDefinitions.Length;
            spawnTemplates = new EnemySpawnRequest[enemyKindCount];
            viewPrefabs = new GameObject[enemyKindCount];
            attackAnimations = new EnemyAttackAnimation[enemyKindCount][];
            attackClips = new AudioClip[enemyKindCount][];
            gaugePrefabs = new GameObject[enemyKindCount];

            for (int kind = 0; kind < enemyKindCount; kind++)
            {
                EnemyDefinition definition = enemyDefinitions[kind];
                EnemyAttackComponent[] attacks = ReadAttacks(definition.name, definition.attacks, projectilePrefabs, hitEffectPrefabs, out attackAnimations[kind], out attackClips[kind]);
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
                    WarnMissingSounds(definition.name, enemyView, attackClips[kind]);
                }
                // 오버레이 체력 바로 체력을 보여 주는 종류는 게이지를 두지 않아도 된다
                if (definition.healthGaugePrefab == null && definition.behavior.showsOverlayHealthBar)
                {
                    continue;
                }
                // 체력 게이지는 표시용이라 빠져 있어도 판은 시작하고, 그 종류는 게이지 없이 둔다
                EnemyHealthGaugeView gaugeView = definition.healthGaugePrefab != null ? definition.healthGaugePrefab.GetComponent<EnemyHealthGaugeView>() : null;
                if (gaugeView == null)
                {
                    Debug.LogWarning($"Runner >> {definition.name}의 Health Gauge Prefab이 없거나 EnemyHealthGaugeView가 붙어 있지 않아 이 적 종류는 체력 게이지 없이 진행한다.");
                }
                else if (!EnemyHealthGaugeViewSystem.HasAllReferences(gaugeView))
                {
                    Debug.LogWarning($"Runner >> {definition.name}의 Health Gauge Prefab에 Gauge Front, Health Text, Level Text 중 연결되지 않은 칸이 있어 이 적 종류는 체력 게이지 없이 진행한다.");
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

        // 효과음 AudioSource가 없거나, 피격 소리가 비어 있거나, 공격 소리가 비어 있으면 한 번씩 경고를 남긴다. 공격 번호는 판에 담은 공격 목록에서의 위치다.
        private static void WarnMissingSounds(string definitionName, EnemyView enemyView, AudioClip[] attackClips)
        {
            if (enemyView.soundEffectSource == null)
            {
                Debug.LogWarning($"Runner >> {definitionName}의 View Prefab에 Sound Effect Source가 연결되어 있지 않아 이 적 종류는 효과음 없이 진행한다.");
                return;
            }

            if (enemyView.hitClip == null)
            {
                Debug.LogWarning($"Runner >> {definitionName}의 View Prefab에 Hit Clip이 없어 이 적 종류는 피격 소리 없이 진행한다.");
            }

            for (int attackIndex = 0; attackIndex < attackClips.Length; attackIndex++)
            {
                if (attackClips[attackIndex] == null)
                {
                    Debug.LogWarning($"Runner >> {definitionName} 공격 {attackIndex}번의 Attack Clip이 없어 이 공격은 소리 없이 진행한다.");
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
