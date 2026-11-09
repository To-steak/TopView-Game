using UnityEngine;
using UnityEngine.Audio;

namespace TopView.Definitions
{
    // UI 버튼 소리. 종류마다 여러 곡 중 하나를 무작위로 고른다
    [CreateAssetMenu(fileName = "User Interface Sound Definition", menuName = "Definitions/User Interface Sound Definition")]
    public class UserInterfaceSoundDefinition : ScriptableObject
    {
        // 일반 버튼을 눌렀을 때 고를 소리들
        public AudioClip[] clickClips;
        // 종료·닫기·나가기 버튼을 눌렀을 때 고를 소리들
        public AudioClip[] closeClips;
        // UI 소리를 내보낼 Audio Mixer 그룹. 비우면 Mixer를 거치지 않는다
        public AudioMixerGroup outputGroup;
        // 소리 볼륨(0~1)
        [Range(0f, 1f)]
        public float volume = 1f;
    }
}
