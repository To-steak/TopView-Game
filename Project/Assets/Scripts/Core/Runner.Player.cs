using System.Collections.Generic;
using TopView.Authoring;
using TopView.Commands;
using TopView.Components;
using TopView.Definitions;
using TopView.Entities;
using TopView.Presentation.Player.Views;
using TopView.Requests;
using TopView.Systems;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TopView.Core
{
    // 플레이어: 스포너와 Definition 읽기, 입력 읽기, 디버그 표시
    public partial class Runner
    {
        private PlayerActions _playerActions;
        private GameObject _playerViewPrefab;
        private int _playerCameraPriority;
        private GameObject[] _playerWeaponModelPrefabs;

        private const int PLAYER_CAPACITY = 1;

        // 씬의 Player Spawner Authoring을 찾는다. 없으면 판을 하지 않는 씬이라 로그 없이 false, 둘 이상이면 false
        private static bool TryFindPlayerSpawner(out PlayerSpawnerAuthoring playerSpawner)
        {
            PlayerSpawnerAuthoring[] playerSpawners = FindObjectsByType<PlayerSpawnerAuthoring>(FindObjectsSortMode.None);
            playerSpawner = null;
            if (playerSpawners.Length == 0)
            {
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
        private static bool TryReadPlayer(PlayerSpawnerAuthoring playerSpawner, List<GameObject> projectilePrefabs, List<float> projectilePitchSpeeds, List<GameObject> hitEffectPrefabs, out PlayerDefinition playerDefinition, out PlayerSpawnRequest spawnRequest, out GameObject[] weaponModelPrefabs)
        {
            spawnRequest = default;
            weaponModelPrefabs = null;
            playerDefinition = playerSpawner.definition;
            if (playerDefinition == null)
            {
                Debug.LogError("Runner >> Player Spawner Authoring에 Player Definition이 연결되어 있지 않아 판을 시작할 수 없다.");
                return false;
            }

            if (!TryReadWeapons(playerDefinition, projectilePrefabs, projectilePitchSpeeds, hitEffectPrefabs, out WeaponComponent[] weapons, out weaponModelPrefabs))
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
        private static bool TryReadWeapons(PlayerDefinition playerDefinition, List<GameObject> projectilePrefabs, List<float> projectilePitchSpeeds, List<GameObject> hitEffectPrefabs, out WeaponComponent[] weapons, out GameObject[] weaponModelPrefabs)
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
                AssignProjectileViews(definition.name, hasProjectile, definition.projectilePrefab, definition.hitEffectPrefab, 0f, ref weapons[weaponIndex].projectile, projectilePrefabs, projectilePitchSpeeds, hitEffectPrefabs);
            }

            return true;
        }

        // 프레임 단위 입력을 읽어 플레이어 명령에 쌓는다. 누름 입력은 틱이 소비할 때까지 남는다.
        private void ReadPlayerInput(ref PlayerCommand command)
        {
            PlayerActions.GlobalActions actions = _playerActions.Global;

            Vector2 moveInput = actions.Move.ReadValue<Vector2>();
            command.moveDirection = ToQuarterDirection(new Vector3(moveInput.x, 0f, moveInput.y));
            command.isRunning = actions.Run.IsPressed();
            command.isAttackHeld = actions.Attack.IsPressed();
            command.hasAimPoint = TryGetAimPoint(out command.aimPoint);

            if (actions.Jump.WasPerformedThisFrame())
            {
                command.isJumpPressed = true;
            }
            if (actions.Dodge.WasPerformedThisFrame())
            {
                command.isDodgePressed = true;
            }
            if (actions.Attack.WasPerformedThisFrame())
            {
                command.isAttackPressed = true;
            }
            if (actions.Reload.WasPerformedThisFrame())
            {
                command.isReloadPressed = true;
            }
            if (actions.Swap.WasPerformedThisFrame())
            {
                command.swapWeaponIndex = (int)actions.Swap.ReadValue<float>() - 1;
            }
        }

        // 입력 방향을 카메라의 앞·오른쪽(XZ) 기준 월드 방향으로 바꾼다.
        private Vector3 ToQuarterDirection(Vector3 input)
        {
            if (_viewContext.mainCamera == null)
            {
                return input;
            }

            Transform cameraTransform = _viewContext.mainCamera.transform;
            Vector3 cameraForward = cameraTransform.forward;
            Vector3 cameraRight = cameraTransform.right;
            cameraForward.y = 0f;
            cameraRight.y = 0f;
            cameraForward.Normalize();
            cameraRight.Normalize();

            return cameraForward * input.z + cameraRight * input.x;
        }

        // 마우스가 가리키는 점을 총구 높이의 수평면 위에서 구한다. 카메라나 플레이어 View가 없으면 false
        private bool TryGetAimPoint(out Vector3 aimPoint)
        {
            aimPoint = Vector3.zero;
            if (_viewContext.mainCamera == null || _viewContext.playerView == null || Mouse.current == null)
            {
                return false;
            }

            Ray ray = _viewContext.mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
            Plane plane = new Plane(Vector3.up, new Vector3(0f, _viewContext.playerView.muzzle.position.y, 0f));
            if (!plane.Raycast(ray, out float distance))
            {
                return false;
            }

            aimPoint = ray.GetPoint(distance);
            return true;
        }

        private bool TryFindAlivePlayer(out int playerIndex)
        {
            for (playerIndex = 0; playerIndex < _world.players.entities.Length; playerIndex++)
            {
                if (_world.players.entities[playerIndex].isAlive)
                {
                    return true;
                }
            }

            playerIndex = -1;
            return false;
        }

#if UNITY_EDITOR
        // 플레이어 이동 충돌에 쓰는 캡슐을 Logic 위치에 그린다. 착지 중이면 초록, 공중이면 노랑
        private void DrawPlayerGizmos()
        {
            PlayerEntity[] players = _world.players.entities;
            for (int playerIndex = 0; playerIndex < players.Length; playerIndex++)
            {
                ref readonly PlayerEntity player = ref players[playerIndex];
                if (!player.isAlive)
                {
                    continue;
                }

                PlayerMovementSystem.GetCapsulePoints(in player, player.currentPosition, out Vector3 top, out Vector3 bottom);
                float radius = player.movement.capsuleRadius;
                Gizmos.color = player.isGrounded ? Color.green : Color.yellow;
                Gizmos.DrawWireSphere(top, radius);
                Gizmos.DrawWireSphere(bottom, radius);
                Gizmos.DrawLine(top + Vector3.forward * radius, bottom + Vector3.forward * radius);
                Gizmos.DrawLine(top + Vector3.back * radius, bottom + Vector3.back * radius);
                Gizmos.DrawLine(top + Vector3.right * radius, bottom + Vector3.right * radius);
                Gizmos.DrawLine(top + Vector3.left * radius, bottom + Vector3.left * radius);
            }
        }
#endif
    }
}
