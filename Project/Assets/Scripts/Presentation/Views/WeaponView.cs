using UnityEngine;

namespace TopView.Presentation.Views
{
    // 무기 모델 프리팹의 참조 묶음
    public class WeaponView : MonoBehaviour
    {
        // 공격이 나갈 때 플레이어의 효과음 AudioSource로 재생할 Clip. 없으면 소리를 내지 않는다
        public AudioClip attackClip;
    }
}
