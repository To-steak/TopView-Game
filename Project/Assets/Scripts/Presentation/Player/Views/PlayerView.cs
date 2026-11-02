using UnityEngine;

namespace TopView.Presentation.Player.Views
{
    // 플레이어 프리팹의 참조 묶음
    public class PlayerView : MonoBehaviour
    {
        // 플레이어 모델의 Animator
        public Animator animator;
        // 무기 모델을 붙일 Transform
        public Transform weaponSocket;
        // 투사체가 나가는 Transform. 조준 평면의 높이도 이 위치를 쓴다
        public Transform muzzle;
    }
}
