using TopView.Presentation.Player.Views;
using UnityEngine;

namespace TopView.Presentation.Camera.ViewSystems
{
    // 카메라 연출을 처리한다.
    public static class CameraViewSystem
    {
        /// <summary>
        /// 플레이어 View가 생기면 플레이어 카메라가 그 View를 따라가게 하고(한 판에 한 번만 정한다), Yaw 입력만큼 플레이어 카메라를 월드 Y축으로 돌린다.
        /// </summary>
        /// <param name="context">카메라 실행 상태</param>
        /// <param name="playerView">따라갈 플레이어 View. 아직 없으면 null</param>
        /// <param name="playerCameraPriority">플레이어 카메라에 줄 Cinemachine 우선순위</param>
        /// <param name="yawInput">Yaw 회전 입력. 양수면 양의 회전, 음수면 음의 회전</param>
        /// <param name="yawDegreesPerSecond">Yaw 회전 속도(도/초)</param>
        public static void Present(CameraViewContext context, PlayerView playerView, int playerCameraPriority, float yawInput, float yawDegreesPerSecond)
        {
            if (context.playerCameraView == null)
            {
                return;
            }

            if (!context.isPlayerCameraFollowing && playerView != null)
            {
                context.playerCameraView.playerCamera.Follow = playerView.transform;
                context.playerCameraView.playerCamera.Priority = playerCameraPriority;
                context.isPlayerCameraFollowing = true;
            }

            context.playerCameraView.playerCamera.transform.Rotate(Vector3.up, yawInput * yawDegreesPerSecond * Time.deltaTime, Space.World);
        }
    }
}
