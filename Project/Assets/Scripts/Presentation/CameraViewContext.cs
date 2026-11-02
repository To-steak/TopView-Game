using TopView.Presentation.Camera.Views;

namespace TopView.Presentation
{
    // 카메라 실행 상태. 판마다 새로 만든다.
    public class CameraViewContext
    {
        // 화면을 그리는 카메라
        public UnityEngine.Camera mainCamera;
        // 플레이어를 따라가는 카메라. 씬에 없으면 null
        public PlayerCameraView playerCameraView;
        // 플레이어 카메라가 따라갈 대상을 이미 정했는지
        public bool isPlayerCameraFollowing;
    }
}
