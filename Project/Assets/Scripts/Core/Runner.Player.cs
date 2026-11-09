using System.Collections.Generic;
using TopView.Authoring;
using TopView.Components;
using TopView.Definitions;
using TopView.Presentation.Views;
using TopView.Requests;
using UnityEngine;

namespace TopView.Core
{
    // 플레이어: 스포너와 Definition 읽기
    public partial class Runner
    {
        private GameObject _playerViewPrefab;
        private int _playerCameraPriority;
        private float _playerCameraYawDegreesPerSecond;
        private int _playerRangedLayerBlendInTicks;
        private float _playerThrowPreviewCircleHeightOffset;
        private GameObject[] _playerWeaponModelPrefabs;

        private const int PLAYER_CAPACITY = 1;

        // 씬의 Player Spawner Authoring을 찾는다. 하나가 아니면 false
        private static bool TryFindPlayerSpawner(out PlayerSpawnerAuthoring playerSpawner)
        {
            PlayerSpawnerAuthoring[] playerSpawners = FindObjectsByType<PlayerSpawnerAuthoring>(FindObjectsSortMode.None);
            playerSpawner = null;
            if (playerSpawners.Length == 0)
            {
                Debug.LogError("Runner >> 씬에 Player Spawner Authoring이 없어 판을 시작할 수 없다.");
                return false;
            }

            if (playerSpawners.Length > 1)
            {
                Debug.LogError($"Runner >> 씬에 Player Spawner Authoring이 하나여야 하는데 {playerSpawners.Length}개라 판을 시작할 수 없다.");
                return false;
            }

            playerSpawner = playerSpawners[0];
            return true;
        }

        // Player Spawner Authoring이 가리키는 Player Definition을 읽어 플레이어 생성 요청을 만든다. 값이 빠져 있으면 false
        private static bool TryReadPlayer(PlayerSpawnerAuthoring playerSpawner, List<GameObject> projectilePrefabs, List<GameObject> hitEffectPrefabs, out PlayerDefinition playerDefinition, out PlayerSpawnRequest spawnRequest, out GameObject[] weaponModelPrefabs)
        {
            spawnRequest = default;
            weaponModelPrefabs = null;
            playerDefinition = playerSpawner.definition;
            if (playerDefinition == null)
            {
                Debug.LogError("Runner >> Player Spawner Authoring에 Player Definition이 연결되어 있지 않아 판을 시작할 수 없다.");
                return false;
            }

            if (!TryReadWeapons(playerDefinition, projectilePrefabs, hitEffectPrefabs, out WeaponComponent[] weapons, out weaponModelPrefabs))
            {
                return false;
            }

            if (playerDefinition.viewPrefab == null || playerDefinition.viewPrefab.GetComponent<PlayerView>() == null)
            {
                Debug.LogError("Runner >> Player Definition의 View Prefab이 없거나 PlayerView가 붙어 있지 않아 판을 시작할 수 없다.");
                return false;
            }

            if (playerDefinition.cameraPriority <= 0)
            {
                Debug.LogWarning("Runner >> Player Definition의 Camera Priority가 0 이하라 플레이어 카메라로 바뀌지 않을 수 있다.");
            }

            if (playerDefinition.cameraYawDegreesPerSecond <= 0f)
            {
                Debug.LogWarning("Runner >> Player Definition의 Camera Yaw Degrees Per Second가 0 이하라 Q/E 회전이 없거나 반대로 돈다.");
            }

            spawnRequest = new PlayerSpawnRequest
            {
                position = playerSpawner.transform.position,
                rotation = Quaternion.identity,
                movement = playerDefinition.movement,
                vitals = playerDefinition.vitals,
                action = playerDefinition.action,
                weapons = weapons
            };
            return true;
        }

        // Player Definition의 무기 목록에서 엔티티에 담을 값과 View가 쓸 모델 프리팹을 꺼내고, 투사체 View와 피격 이펙트를 표에 올린다. 빈 칸이 있으면 false
        private static bool TryReadWeapons(PlayerDefinition playerDefinition, List<GameObject> projectilePrefabs, List<GameObject> hitEffectPrefabs, out WeaponComponent[] weapons, out GameObject[] weaponModelPrefabs)
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
                AssignProjectileViews(definition.name, hasProjectile, definition.projectilePrefab, definition.hitEffectPrefab, ref weapons[weaponIndex].projectile, projectilePrefabs, hitEffectPrefabs);
            }

            return true;
        }
    }
}
