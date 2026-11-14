using System.Collections.Generic;
using TopView.Commands;
using TopView.Components;
using TopView.Entities;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TopView.Core
{
    // 입력: 프레임 단위 입력을 읽어 플레이어 명령에 쌓는다
    public partial class World
    {
        private PlayerActions _playerActions;

        // ESC를 누르면 설정 화면이 열려 있으면 닫고, 강화 화면이 열려 있으면 닫고, 일시정지 중이면 풀고, 아니면 일시정지한다. 판이 끝난 뒤에는 정상적으로 무시한다
        private void ReadEscapeInput(CommandBuffer commandBuffer, List<SettingsCommand> settingsCommands, bool isSettingsOpen, in GameProgressComponent progress)
        {
            if (!_playerActions.UserInterface.Escape.WasPerformedThisFrame() || progress.phase != GamePhase.Playing)
            {
                return;
            }

            if (isSettingsOpen)
            {
                settingsCommands.Add(new SettingsCommand { type = SettingsCommandType.Close });
                return;
            }

            if (progress.isUpgradeOpen)
            {
                commandBuffer.pauseCommands.Add(new PauseCommand { type = PauseCommandType.CloseUpgrade });
                return;
            }

            PauseCommandType type = progress.isPaused ? PauseCommandType.Resume : PauseCommandType.Pause;
            commandBuffer.pauseCommands.Add(new PauseCommand { type = type });
        }

        // Tab을 누르면 강화 화면이 열려 있으면 닫고, 일시정지 중이 아니면 연다. ESC로 일시정지한 중이나 판이 끝난 뒤에는 정상적으로 무시한다
        private void ReadUpgradeInput(CommandBuffer commandBuffer, in GameProgressComponent progress)
        {
            if (!_playerActions.UserInterface.Upgrade.WasPerformedThisFrame() || progress.phase != GamePhase.Playing)
            {
                return;
            }

            if (progress.isUpgradeOpen)
            {
                commandBuffer.pauseCommands.Add(new PauseCommand { type = PauseCommandType.CloseUpgrade });
            }
            else if (!progress.isPaused)
            {
                commandBuffer.pauseCommands.Add(new PauseCommand { type = PauseCommandType.OpenUpgrade });
            }
        }

        // 일시정지 중에는 플레이어 입력을 읽지 않고 누르고 있는 입력을 비운다. 카메라 회전처럼 틱 밖에서 쓰는 입력이 계속 남지 않게 한다
        private static void ClearHeldPlayerInput(ref PlayerCommand command)
        {
            command.moveDirection = Vector3.zero;
            command.isRunning = false;
            command.isAttackHeld = false;
            command.cameraYawInput = 0f;
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
            command.cameraYawInput = actions.CameraYaw.ReadValue<float>();

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
            if (actions.Cancel.WasPerformedThisFrame())
            {
                command.isCancelPressed = true;
            }
            if (actions.Swap.WasPerformedThisFrame())
            {
                command.swapWeaponIndex = (int)actions.Swap.ReadValue<float>() - 1;
            }
        }

        // 입력 방향을 카메라의 앞·오른쪽(XZ) 기준 월드 방향으로 바꾼다.
        private Vector3 ToQuarterDirection(Vector3 input)
        {
            if (_viewContext.camera.mainCamera == null)
            {
                return input;
            }

            Transform cameraTransform = _viewContext.camera.mainCamera.transform;
            Vector3 cameraForward = cameraTransform.forward;
            Vector3 cameraRight = cameraTransform.right;
            cameraForward.y = 0f;
            cameraRight.y = 0f;
            cameraForward.Normalize();
            cameraRight.Normalize();

            return cameraForward * input.z + cameraRight * input.x;
        }

        // 마우스가 가리키는 점을 든 무기의 조준 높이의 수평면 위에서 구한다. 카메라나 살아 있는 플레이어가 없으면 false
        private bool TryGetAimPoint(out Vector3 aimPoint)
        {
            aimPoint = Vector3.zero;
            if (_viewContext.camera.mainCamera == null || Mouse.current == null || !TryFindAlivePlayer(out int playerIndex))
            {
                return false;
            }

            Ray ray = _viewContext.camera.mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
            Plane plane = new Plane(Vector3.up, new Vector3(0f, GetAimHeight(in _entities.entities[playerIndex], _entities.entities), 0f));
            if (!plane.Raycast(ray, out float distance))
            {
                return false;
            }

            aimPoint = ray.GetPoint(distance);
            return true;
        }

        // 살아 있는(Die가 아닌) 플레이어를 찾는다. 사망한 플레이어는 조준하지 않는다
        private bool TryFindAlivePlayer(out int playerIndex)
        {
            Entity[] entities = _entities.entities;
            for (playerIndex = 0; playerIndex < entities.Length; playerIndex++)
            {
                if ((entities[playerIndex].mask & ComponentMask.PlayerActionState) != 0 && entities[playerIndex].playerActionState.state != PlayerState.Die)
                {
                    return true;
                }
            }

            playerIndex = -1;
            return false;
        }

        // 든 무기의 조준 높이(월드). 플레이어 발 높이에 무기의 aimHeight를 더한다. 든 무기가 없으면 플레이어 위치의 높이
        private static float GetAimHeight(in Entity entity, Entity[] weapons)
        {
            EntityIdentifier[] weaponIdentifiers = entity.weaponSlots.weapons;
            int weaponIndex = entity.weaponSlots.currentIndex;
            if (weaponIdentifiers == null || weaponIndex < 0 || weaponIndex >= weaponIdentifiers.Length)
            {
                return entity.position.current.y;
            }

            return entity.position.current.y + weapons[weaponIdentifiers[weaponIndex].index].weapon.aimHeight;
        }
    }
}
