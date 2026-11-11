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
        // 점프 상태에 들어갈 때 Sound Effect Source로 재생할 소리. 없으면 null
        public AudioClip jumpClip;
        // 회피(구르기) 상태에 들어갈 때 Sound Effect Source로 재생할 소리. 없으면 null
        public AudioClip dodgeClip;
        // 투척 충전 중의 궤도를 그리는 LineRenderer. Use World Space를 켜 둔다
        public LineRenderer throwPreviewLine;
        // 투척 충전 중에 폭발 범위를 보여 주는 원의 Root. 회전하지 않은 Root 아래에 바닥으로 눕힌 1×1 Quad를 두고, Quad는 가장자리까지 꽉 찬 링 텍스처를 쓴다. 코드는 Root의 X, Z 크기를 지름으로 바꾼다
        public Transform throwPreviewCircle;
    }
}
