using UnityEngine;
using UnityEngine.Audio;

namespace TopView.Definitions
{
    // 배경음악 곡과 재생 값
    [CreateAssetMenu(fileName = "Music Definition", menuName = "Definitions/Music Definition")]
    public class MusicDefinition : ScriptableObject
    {
        // 평소에 반복 재생하는 곡
        public AudioClip normalClip;
        // 판에서 화면 체력바를 띄우는 적(Shows Overlay Health Bar)이 처음 나타나면 바꿔 트는 곡. 판이 끝날 때까지 이어 튼다. 비우면 평소 곡을 계속 튼다
        public AudioClip bossClip;
        // 배경음악을 내보낼 Audio Mixer 그룹. 비우면 Mixer를 거치지 않는다
        public AudioMixerGroup outputGroup;
        // 곡이 다 들렸을 때의 AudioSource 볼륨(0~1)
        [Range(0f, 1f)]
        public float volume = 1f;
        // 곡을 페이드 인하는 데, 보스 곡으로 크로스페이드하는 데 걸리는 틱 수. 0이면 바로 바뀐다
        public int fadeTicks;
    }
}
