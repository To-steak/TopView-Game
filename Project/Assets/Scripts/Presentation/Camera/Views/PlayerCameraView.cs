using Unity.Cinemachine;
using UnityEngine;

namespace TopView.Presentation.Camera.Views
{
    // 플레이어를 따라가는 카메라의 참조 묶음
    public class PlayerCameraView : MonoBehaviour
    {
        // 플레이어를 따라갈 Cinemachine 카메라
        public CinemachineCamera playerCamera;
    }
}
