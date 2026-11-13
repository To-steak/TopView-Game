using System.Collections.Generic;
using TopView.Authoring;
using TopView.Components;
using TopView.Definitions;
using TopView.Entities;
using TopView.Presentation.Views;
using TopView.Requests;
using UnityEngine;

namespace TopView.Core
{
    // 플레이어: 스포너와 Definition 읽기
    public partial class World
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
                Debug.LogError("World >> 씬에 Player Spawner Authoring이 없어 판을 시작할 수 없다.");
                return false;
            }

            if (playerSpawners.Length > 1)
            {
                Debug.LogError($"World >> 씬에 Player Spawner Authoring이 하나여야 하는데 {playerSpawners.Length}개라 판을 시작할 수 없다.");
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
                Debug.LogError("World >> Player Spawner Authoring에 Player Definition이 연결되어 있지 않아 판을 시작할 수 없다.");
                return false;
            }

            if (!TryReadWeapons(playerDefinition, projectilePrefabs, hitEffectPrefabs, out WeaponSpawnRequest[] weapons, out weaponModelPrefabs))
            {
                return false;
            }

            if (playerDefinition.viewPrefab == null || playerDefinition.viewPrefab.GetComponent<PlayerView>() == null)
            {
                Debug.LogError("World >> Player Definition의 View Prefab이 없거나 PlayerView가 붙어 있지 않아 판을 시작할 수 없다.");
                return false;
            }

            if (playerDefinition.cameraPriority <= 0)
            {
                Debug.LogWarning("World >> Player Definition의 Camera Priority가 0 이하라 플레이어 카메라로 바뀌지 않을 수 있다.");
            }

            if (playerDefinition.cameraYawDegreesPerSecond <= 0f)
            {
                Debug.LogWarning("World >> Player Definition의 Camera Yaw Degrees Per Second가 0 이하라 Q/E 회전이 없거나 반대로 돈다.");
            }

            spawnRequest = new PlayerSpawnRequest
            {
                position = playerSpawner.transform.position,
                rotation = Quaternion.identity,
                movement = playerDefinition.movement,
                vitals = playerDefinition.vitals,
                action = playerDefinition.action,
                body = playerDefinition.body,
                death = playerDefinition.death,
                upgrade = playerDefinition.upgrade,
                weapons = weapons
            };
            return true;
        }

        // Player Definition의 무기 목록에서 무기 생성 요청과 View가 쓸 모델 프리팹을 꺼내고, 투사체 View와 피격 이펙트, 근접 임팩트 이펙트를 표에 올린다. 빈 칸이 있으면 false
        private static bool TryReadWeapons(PlayerDefinition playerDefinition, List<GameObject> projectilePrefabs, List<GameObject> hitEffectPrefabs, out WeaponSpawnRequest[] weapons, out GameObject[] weaponModelPrefabs)
        {
            WeaponDefinition[] weaponDefinitions = playerDefinition.weapons;
            weapons = new WeaponSpawnRequest[weaponDefinitions.Length];
            weaponModelPrefabs = new GameObject[weaponDefinitions.Length];
            if (weaponDefinitions.Length == 0)
            {
                Debug.LogError("World >> Player Definition의 무기 목록이 비어 있어 판을 시작할 수 없다.");
                return false;
            }

            for (int weaponIndex = 0; weaponIndex < weaponDefinitions.Length; weaponIndex++)
            {
                WeaponDefinition definition = weaponDefinitions[weaponIndex];
                if (definition == null)
                {
                    Debug.LogError($"World >> Player Definition의 무기 {weaponIndex}번 칸이 비어 있어 판을 시작할 수 없다.");
                    return false;
                }

                if (TryFindWeaponShotsProblem(definition, out string shotsProblem))
                {
                    Debug.LogError($"World >> {definition.name}의 {shotsProblem}. 판을 시작할 수 없다.");
                    return false;
                }

                if (!TryCreateWeaponSpawnRequest(definition, out weapons[weaponIndex]))
                {
                    Debug.LogError($"World >> {definition.name}의 무기 종류({definition.kind})를 처리하지 않아 판을 시작할 수 없다.");
                    return false;
                }
                weaponModelPrefabs[weaponIndex] = definition.modelPrefab;

                bool hasProjectile = definition.kind != WeaponKind.Melee;
                AssignProjectileViews(definition.name, hasProjectile, definition.projectilePrefab, definition.hitEffectPrefab, ref weapons[weaponIndex].projectileView, projectilePrefabs, hitEffectPrefabs);
                weapons[weaponIndex].meleeView.impactEffectIndex = hasProjectile ? -1 : IndexHitEffect($"{definition.name}의 임팩트 이펙트", definition.impactEffectPrefab, hitEffectPrefabs);
            }

            return true;
        }

        // 무기 종류로 무기가 가질 컴포넌트와 공격 키를 누를 때 들어갈 행동 상태를 정해 주인 없는 생성 요청을 만든다. 처리하지 않는 종류면 false
        private static bool TryCreateWeaponSpawnRequest(WeaponDefinition definition, out WeaponSpawnRequest request)
        {
            request = new WeaponSpawnRequest
            {
                mask = ComponentMask.Owner | ComponentMask.Weapon | ComponentMask.UpgradeLevel | ComponentMask.WeaponUpgrade | ComponentMask.AttackState,
                weapon = definition.weapon,
                upgrade = definition.upgrade
            };

            switch (definition.kind)
            {
                case WeaponKind.Melee:
                    request.mask |= ComponentMask.MeleeAttack | ComponentMask.MeleeView | ComponentMask.AttackInterval | ComponentMask.AttackDuration;
                    request.attackState.state = PlayerState.Swing;
                    request.meleeAttack = definition.meleeAttack;
                    request.attackInterval = definition.attackInterval;
                    request.attackDuration = definition.attackDuration;
                    return true;

                case WeaponKind.Ranged:
                    request.mask |= ComponentMask.StraightAttack | ComponentMask.ProjectileView | ComponentMask.Magazine | ComponentMask.MagazineState
                        | ComponentMask.ReserveAmmo | ComponentMask.ReserveAmmoState | ComponentMask.AttackInterval;
                    request.attackState.state = PlayerState.Shot;
                    request.straightAttack = definition.straightAttack;
                    request.magazine = definition.magazine;
                    request.reserveAmmo = definition.reserveAmmo;
                    request.attackInterval = definition.attackInterval;
                    return true;

                case WeaponKind.Throw:
                    request.mask |= ComponentMask.ArcAttack | ComponentMask.Throw | ComponentMask.ProjectileView | ComponentMask.ReserveAmmo
                        | ComponentMask.ReserveAmmoState | ComponentMask.AttackDuration;
                    request.attackState.state = PlayerState.ThrowCharge;
                    request.arcAttack = definition.arcAttack;
                    request.throwing = definition.throwing;
                    request.reserveAmmo = definition.reserveAmmo;
                    request.attackDuration = definition.attackDuration;
                    return true;
            }

            return false;
        }

        // 투사체 무기의 발 목록에 문제가 있으면 적어 true를 돌려준다. Ranged는 마지막 발에서 공격 상태가 끝나 Duration Ticks와 비교하지 않는다. Melee는 발 목록이 없어 false다.
        private static bool TryFindWeaponShotsProblem(WeaponDefinition definition, out string problem)
        {
            switch (definition.kind)
            {
                case WeaponKind.Ranged:
                    return TryFindShotsProblem(definition.straightAttack.shots, false, 0, out problem);
                case WeaponKind.Throw:
                    return TryFindShotsProblem(definition.arcAttack.shots, true, definition.attackDuration.durationTicks, out problem);
            }

            problem = null;
            return false;
        }
    }
}
