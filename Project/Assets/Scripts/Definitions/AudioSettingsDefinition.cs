using TopView.Components;
using UnityEngine;
using UnityEngine.Audio;

namespace TopView.Definitions
{
    // 볼륨을 조절할 Audio Mixer와 볼륨 설정값
    [CreateAssetMenu(fileName = "Audio Settings Definition", menuName = "Definitions/Audio Settings Definition")]
    public class AudioSettingsDefinition : ScriptableObject
    {
        // 배경음악과 효과음 그룹의 볼륨이 노출된 Audio Mixer
        public AudioMixer mixer;
        // 볼륨 시작값
        public AudioSettingsComponent settings = new AudioSettingsComponent
        {
            startingMusicVolume = 1f,
            startingSoundEffectVolume = 1f
        };
    }
}
