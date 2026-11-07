using UnityEngine;

namespace TopView.Presentation.Views
{
    // 플레이어 프리팹의 참조 묶음
    public class PlayerView : MonoBehaviour
    {
        // 플레이어 모델의 Animator
        public Animator animator;
        // 무기 모델을 붙일 Transform
        public Transform weaponSocket;
        // 플레이어의 효과음을 재생하는 AudioSource. Output은 Audio Mixer의 효과음 그룹으로 둔다
        public AudioSource soundEffectSource;
    }
}
