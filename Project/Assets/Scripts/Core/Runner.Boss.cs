using System.Collections.Generic;
using TopView.Authoring;
using TopView.Components;
using TopView.Definitions;
using TopView.Entities;
using TopView.Presentation.Boss.Views;
using TopView.Requests;
using UnityEngine;

namespace TopView.Core
{
    // 보스: 스포너와 Definition 읽기
    public partial class Runner
    {
        private GameObject _bossViewPrefab;

        private const int BOSS_CAPACITY = 1;

        // 씬의 Boss Spawner Authoring이 가리키는 Boss Definition을 읽어 보스 생성 템플릿을 만든다. 스포너가 하나가 아니거나 값이 빠져 있으면 false
        private static bool TryReadBoss(List<GameObject> projectilePrefabs, List<float> projectilePitchSpeeds, List<GameObject> hitEffectPrefabs, out BossDefinition bossDefinition, out BossSpawnRequest spawnTemplate)
        {
            bossDefinition = null;
            spawnTemplate = default;

            BossSpawnerAuthoring[] bossSpawners = FindObjectsByType<BossSpawnerAuthoring>(FindObjectsSortMode.None);
            if (bossSpawners.Length != 1)
            {
                Debug.LogError($"Runner >> 씬에 Boss Spawner Authoring이 하나여야 하는데 {bossSpawners.Length}개라 판을 시작할 수 없다.");
                return false;
            }

            BossSpawnerAuthoring bossSpawner = bossSpawners[0];
            bossDefinition = bossSpawner.definition;
            if (bossDefinition == null || bossDefinition.viewPrefab == null || bossDefinition.viewPrefab.GetComponent<BossView>() == null)
            {
                Debug.LogError("Runner >> Boss Spawner Authoring에 Boss Definition이 없거나, View Prefab이 없거나 BossView가 붙어 있지 않아 판을 시작할 수 없다.");
                return false;
            }

            EnemyAttackComponent[] attacks = (EnemyAttackComponent[])bossDefinition.attacks.Clone();
            AssignAttackViews(bossDefinition.name, attacks, bossDefinition.attackProjectilePrefabs, bossDefinition.attackHitEffectPrefabs, bossDefinition.attackProjectilePitchSpeeds, projectilePrefabs, projectilePitchSpeeds, hitEffectPrefabs);

            spawnTemplate = new BossSpawnRequest
            {
                position = bossSpawner.transform.position,
                stats = bossDefinition.stats,
                behavior = bossDefinition.behavior,
                attacks = attacks
            };
            return true;
        }

#if UNITY_EDITOR
        // 살아 있는 보스의 Body 공격 판정 원을 Logic 위치에 그린다.
        private void DrawBossGizmos()
        {
            BossEntity[] bosses = _world.bosses.entities;
            for (int bossIndex = 0; bossIndex < bosses.Length; bossIndex++)
            {
                ref readonly BossEntity boss = ref bosses[bossIndex];
                if (boss.isAlive)
                {
                    DrawBodyAttackGizmos(boss.attacks, boss.currentPosition, boss.currentRotation);
                }
            }
        }
#endif
    }
}
