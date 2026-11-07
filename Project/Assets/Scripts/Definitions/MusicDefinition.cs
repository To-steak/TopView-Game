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
        // 배경음악을 내보낼 Audio Mixer 그룹. 비우면 Mixer를 거치지 않는다
        public AudioMixerGroup outputGroup;
        // 곡이 다 들렸을 때의 AudioSource 볼륨(0~1)
        [Range(0f, 1f)]
        public float volume = 1f;
        // 곡을 페이드 인하는 데 걸리는 틱 수. 0이면 바로 다 들린다
        public int fadeTicks;
    }
}
