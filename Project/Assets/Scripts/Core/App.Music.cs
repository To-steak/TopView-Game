using TopView.Definitions;
using TopView.Presentation.ViewContexts;
using UnityEngine;
using UnityEngine.Audio;

namespace TopView.Core
{
    // 배경음악: 씬을 등록할 때 그 씬이 넘긴 Music Definition으로 곡의 AudioSource를 만든다
    public partial class App
    {
        // 지금 씬의 배경음악 실행 상태. 씬의 Music Definition이나 평소 곡이 없으면 null이고, 이때는 배경음악을 틀지 않는다
        private MusicViewContext _musicViewContext;

        private const string MUSIC_OBJECT_NAME = "Music";

        // 지금 씬에 배경음악 오브젝트를 만들어 평소 곡과 보스 곡의 AudioSource를 둔다. 보스 곡을 쓰지 않거나 없으면 보스 곡 AudioSource는 null이다.
        // 씬에 만들므로 씬이 바뀌면 함께 사라지고, 새 씬은 곡을 처음부터 튼다.
        // Music Definition이나 평소 곡이 없으면 null을 돌려준다.
        private static MusicViewContext CreateMusicViewContext(MusicDefinition definition, bool usesBossClip)
        {
            if (definition == null)
            {
                Debug.LogError("App >> 씬에 Music Definition이 연결되어 있지 않아 배경음악이 나오지 않는다.");
                return null;
            }

            if (definition.normalClip == null)
            {
                Debug.LogError("App >> Music Definition에 Normal Clip이 연결되어 있지 않아 배경음악이 나오지 않는다.");
                return null;
            }

            if (usesBossClip && definition.bossClip == null)
            {
                Debug.LogWarning("App >> Music Definition에 Boss Clip이 연결되어 있지 않아 보스가 나와도 평소 곡을 계속 튼다.");
            }

            if (definition.outputGroup == null)
            {
                Debug.LogWarning("App >> Music Definition에 Output Group이 연결되어 있지 않아 배경음악이 Audio Mixer를 거치지 않는다.");
            }

            if (definition.fadeTicks < 0)
            {
                Debug.LogWarning("App >> Music Definition의 Fade Ticks가 0보다 작아 페이드 없이 곡을 튼다.");
            }

            GameObject musicObject = new GameObject(MUSIC_OBJECT_NAME);
            bool hasBossSource = usesBossClip && definition.bossClip != null;
            return new MusicViewContext
            {
                normalSource = CreateMusicSource(musicObject, definition.normalClip, definition.outputGroup),
                bossSource = hasBossSource ? CreateMusicSource(musicObject, definition.bossClip, definition.outputGroup) : null,
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
