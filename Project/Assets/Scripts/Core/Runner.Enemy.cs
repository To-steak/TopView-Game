using System.Collections.Generic;
using TopView.Authoring;
using TopView.Components;
using TopView.Definitions;
using TopView.Entities;
using TopView.Presentation.Enemy.Views;
using TopView.Requests;
using UnityEngine;

namespace TopView.Core
{
    // 적: 웨이브의 적 종류, Definition, 스폰 지점 읽기
    public partial class Runner
    {
        // 적 종류별 View 프리팹. 번호가 적 종류 번호다
        private GameObject[] _enemyViewPrefabs;
        // 적 종류별 체력 게이지 프리팹. 번호가 적 종류 번호다
        private GameObject[] _enemyHealthGaugePrefabs;

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
                    attacks = attacks
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

#if UNITY_EDITOR
        // 살아 있는 적마다 감지 반경(노랑), 멈추는 거리(주황), 피격 판정 원(청록), Body 공격 판정 원(빨강)을 Logic 위치에 XZ 원으로 그린다.
        // 감지와 공격 판정은 여기에 플레이어 반지름을 더해 비교하므로, 원은 플레이어 중심이 아니라 가장자리가 닿는 기준이다
        private void DrawEnemyGizmos()
        {
            EnemyEntity[] enemies = _world.enemies.entities;
            for (int enemyIndex = 0; enemyIndex < enemies.Length; enemyIndex++)
            {
                ref readonly EnemyEntity enemy = ref enemies[enemyIndex];
                if (!enemy.isAlive)
                {
                    continue;
                }

                Gizmos.color = Color.yellow;
                DrawCircleGizmo(enemy.currentPosition, enemy.behavior.detectionRadius);
                Gizmos.color = new Color(1f, 0.5f, 0f);
                DrawCircleGizmo(enemy.currentPosition, enemy.behavior.stopDistance);
                Gizmos.color = Color.cyan;
                DrawCircleGizmo(enemy.currentPosition, enemy.stats.hitRadius);
                DrawBodyAttackGizmos(enemy.attacks, enemy.currentPosition, enemy.currentRotation);
            }
        }

        // 공격 목록의 Body 공격마다 공격이 나갈 때의 판정 원을 빨강으로 그린다.
        private static void DrawBodyAttackGizmos(EnemyAttackComponent[] attacks, Vector3 position, Quaternion rotation)
        {
            Gizmos.color = Color.red;
            for (int attackIndex = 0; attackIndex < attacks.Length; attackIndex++)
            {
                ref readonly EnemyAttackComponent attack = ref attacks[attackIndex];
                if (attack.kind == EnemyAttackKind.Body)
                {
                    DrawCircleGizmo(position + rotation * Vector3.forward * attack.bodyOffset, attack.bodyRadius);
                }
            }
        }
#endif
    }
}
