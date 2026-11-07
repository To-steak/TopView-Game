using UnityEngine;

namespace TopView.Presentation.Views
{
    // 무기 모델 프리팹의 참조 묶음
    public class WeaponView : MonoBehaviour
    {
        // 공격이 나갈 때 플레이어의 효과음 AudioSource로 재생할 Clip. 없으면 소리를 내지 않는다
        public AudioClip attackClip;
        // 장전 중 정해진 틱마다 플레이어의 효과음 AudioSource로 재생할 소리. startTicks 오름차순으로 둔다. Ranged만 쓴다. 비우면 소리를 내지 않는다
        public TimedSoundEntry[] reloadSounds;
        // 투사체가 나가는 자리. 에디터에서 WeaponDefinition의 muzzleOffset을 구울 때만 읽고, 게임 중에는 쓰지 않는다
        public Transform muzzle;
    }
}
