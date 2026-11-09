using Unity.Cinemachine;
using UnityEngine;

namespace TopView.Presentation.Views
{
    // 플레이어를 따라가는 카메라의 참조 묶음
    public class PlayerCameraView : MonoBehaviour
    {
        // 플레이어를 따라갈 Cinemachine 카메라
        public CinemachineCamera playerCamera;
        // 플레이어 카메라의 방향을 정하는 Pan Tilt. Q/E 입력은 이 Pan 값을 돌린다
        public CinemachinePanTilt playerCameraPanTilt;
    }
}
