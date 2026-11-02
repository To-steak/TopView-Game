using TopView.Commands;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TopView.Core
{
    // 입력: 프레임 단위 입력을 읽어 플레이어 명령에 쌓는다
    public partial class Runner
    {
        private PlayerActions _playerActions;

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

        // 마우스가 가리키는 점을 총구 높이의 수평면 위에서 구한다. 카메라나 플레이어 View가 없으면 false
        private bool TryGetAimPoint(out Vector3 aimPoint)
        {
            aimPoint = Vector3.zero;
            if (_viewContext.camera.mainCamera == null || _viewContext.player.view == null || Mouse.current == null)
            {
                return false;
            }

            Ray ray = _viewContext.camera.mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
            Plane plane = new Plane(Vector3.up, new Vector3(0f, _viewContext.player.view.muzzle.position.y, 0f));
            if (!plane.Raycast(ray, out float distance))
            {
                return false;
            }

            aimPoint = ray.GetPoint(distance);
            return true;
        }
    }
}
