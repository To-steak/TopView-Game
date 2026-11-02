using TopView.Presentation.Player.Views;

namespace TopView.Presentation.Camera.ViewSystems
{
    // 카메라 연출을 처리한다.
    public static class CameraViewSystem
    {
        /// <summary>
        /// 플레이어 View가 생기면 플레이어 카메라가 그 View를 따라가게 한다. 한 판에 한 번만 정한다.
        /// </summary>
        /// <param name="context">카메라 실행 상태</param>
        /// <param name="playerView">따라갈 플레이어 View. 아직 없으면 null</param>
        /// <param name="playerCameraPriority">플레이어 카메라에 줄 Cinemachine 우선순위</param>
        public static void Present(CameraViewContext context, PlayerView playerView, int playerCameraPriority)
        {
            if (context.isPlayerCameraFollowing || playerView == null || context.playerCameraView == null)
            {
                return;
            }

            context.playerCameraView.playerCamera.Follow = playerView.transform;
            context.playerCameraView.playerCamera.Priority = playerCameraPriority;
            context.isPlayerCameraFollowing = true;
        }
    }
}
