using UnityEngine;

namespace TopView.Presentation.Views
{
    // 적 프리팹의 참조 묶음
    public class EnemyView : MonoBehaviour
    {
        // 적 모델의 Animator
        public Animator animator;
        // 체력 게이지를 띄울 위치. 루트 바로 아래에 두어 애니메이션을 따라 흔들리지 않게 한다
        public Transform healthGaugeAnchor;
        // 피해량 글자를 띄울 위치. 루트 바로 아래에 두어 애니메이션을 따라 흔들리지 않게 한다
        public Transform damageAnchor;
        // 적의 효과음(피격, 공격)을 재생하는 AudioSource. 적의 자리에서 들리도록 Spatial Blend를 3D로 두고, Play On Awake와 Loop는 끈다. Output은 Audio Mixer의 효과음 그룹으로 둔다
        public AudioSource soundEffectSource;
        // 피해를 받을 때 재생할 소리. 없으면 null
        public AudioClip hitClip;
        // 투사체가 나가는 자리들. 에디터에서 Enemy Definition의 Shots 칸마다 Spawn Offset을 구울 때만 읽고, 게임 중에는 쓰지 않는다
        public Transform[] muzzles;
    }
}
