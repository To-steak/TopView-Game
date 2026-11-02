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
    // 적: 스포너의 적 종류와 스폰 지점, Definition 읽기
    public partial class Runner
    {
        // 적 종류별 View 프리팹. 번호가 적 종류 번호다
        private GameObject[] _enemyViewPrefabs;
        // 적 종류별 체력 게이지 프리팹. 번호가 적 종류 번호다
        private GameObject[] _enemyHealthGaugePrefabs;

        // 씬의 켜진 Spawner Authoring을 이름순으로 읽어 적 종류 목록과 스폰 지점 목록을 만든다. 적 종류 번호는 스포너 이름순에서 처음 나온 순서다. 같은 시드에서 같은 결과가 나오도록 순서를 고정한다.
        private static void ReadEnemySpawners(out EnemyDefinition[] enemyDefinitions, out SpawnPointComponent[] spawnPoints)
        {
            EnemySpawnerAuthoring[] spawners = FindObjectsByType<EnemySpawnerAuthoring>(FindObjectsSortMode.None);
            SortByName(spawners);

            List<EnemyDefinition> kinds = new List<EnemyDefinition>();
            List<SpawnPointComponent> spawnPointList = new List<SpawnPointComponent>();
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
            }

            if (spawnPointList.Count == 0)
            {
                Debug.LogWarning("Runner >> 씬에 켜진 Enemy Spawner Authoring이 없어 적이 생기지 않는다.");
            }

            enemyDefinitions = kinds.ToArray();
            spawnPoints = spawnPointList.ToArray();
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
            EntityStorage entities = _world.entities;
            for (int index = 0; index < entities.masks.Length; index++)
            {
                if ((entities.masks[index] & ComponentMask.EnemyArchetype) != ComponentMask.EnemyArchetype)
                {
                    continue;
                }

                Vector3 position = entities.positions[index].current;
                Gizmos.color = Color.yellow;
                DrawCircleGizmo(position, entities.enemyBehaviors[index].detectionRadius);
                Gizmos.color = new Color(1f, 0.5f, 0f);
                DrawCircleGizmo(position, entities.enemyBehaviors[index].stopDistance);
                Gizmos.color = Color.cyan;
                DrawCircleGizmo(position, entities.enemyStats[index].hitRadius);
                DrawBodyAttackGizmos(entities.attackLists[index].attacks, position, entities.rotations[index].current);
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
