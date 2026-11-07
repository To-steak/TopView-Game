using TopView.Definitions;
using TopView.Presentation.ViewContexts;
using UnityEngine;
using UnityEngine.Audio;

namespace TopView.Core
{
    // 배경음악: 판마다 AudioSource를 만들고 Music Definition 값을 담는다
    public partial class Runner
    {
        // 배경음악 곡과 재생 값
        [SerializeField] private MusicDefinition _musicDefinition;

        private const string MUSIC_OBJECT_NAME = "Music";

        // 활성 씬에 배경음악 오브젝트를 만들어 평소 곡의 AudioSource를 둔다.
        // 씬에 만들므로 재시작으로 씬을 다시 불러오면 함께 사라지고, 새 판은 곡을 처음부터 튼다.
        // Music Definition이나 평소 곡이 없으면 null을 돌려준다.
        private MusicViewContext CreateMusicViewContext()
        {
            MusicDefinition definition = _musicDefinition;
            if (definition == null)
            {
                Debug.LogError("Runner >> Runner에 Music Definition이 연결되어 있지 않아 배경음악이 나오지 않는다.");
                return null;
            }

            if (definition.normalClip == null)
            {
                Debug.LogError("Runner >> Music Definition에 Normal Clip이 연결되어 있지 않아 배경음악이 나오지 않는다.");
                return null;
            }

            if (definition.outputGroup == null)
            {
                Debug.LogWarning("Runner >> Music Definition에 Output Group이 연결되어 있지 않아 배경음악이 Audio Mixer를 거치지 않는다.");
            }

            if (definition.fadeTicks < 0)
            {
                Debug.LogWarning("Runner >> Music Definition의 Fade Ticks가 0보다 작아 페이드 없이 곡을 튼다.");
            }

            GameObject musicObject = new GameObject(MUSIC_OBJECT_NAME);
            return new MusicViewContext
            {
                normalSource = CreateMusicSource(musicObject, definition.normalClip, definition.outputGroup),
                volume = definition.volume,
                fadeTicks = definition.fadeTicks
            };
        }

        private static AudioSource CreateMusicSource(GameObject musicObject, AudioClip clip, AudioMixerGroup outputGroup)
        {
            AudioSource source = musicObject.AddComponent<AudioSource>();
            source.clip = clip;
            source.outputAudioMixerGroup = outputGroup;
            source.loop = true;
            source.playOnAwake = false;
            source.spatialBlend = 0f;
            source.volume = 0f;
            return source;
        }
    }
}
