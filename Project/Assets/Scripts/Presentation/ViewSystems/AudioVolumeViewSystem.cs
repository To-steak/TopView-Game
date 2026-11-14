using TopView.Components;
using TopView.Presentation.ViewContexts;
using UnityEngine;

namespace TopView.Presentation.ViewSystems
{
    // 볼륨(0~1)을 데시벨로 바꿔 Audio Mixer의 노출된 파라미터에 넣는다.
    public static class AudioVolumeViewSystem
    {
        // Audio Mixer에서 Music 그룹 Volume을 노출한 이름
        private const string MUSIC_VOLUME_PARAMETER = "MusicVolume";
        // Audio Mixer에서 SFX 그룹 Volume을 노출한 이름
        private const string SOUND_EFFECT_VOLUME_PARAMETER = "SoundEffectVolume";
        // Audio Mixer 그룹 Volume의 최솟값. 볼륨 0은 이 값으로 소리를 끈다
        private const float MINIMUM_DECIBELS = -80f;

        /// <summary>
        /// 볼륨을 Audio Mixer에 반영한다. Start 이후에 불러야 한다(Awake에서는 Mixer가 값을 무시할 수 있다).
        /// 반영하지 못하면 처음 한 번만 경고한다.
        /// </summary>
        /// <param name="volume">반영할 볼륨</param>
        /// <param name="context">볼륨 반영 실행 상태</param>
        public static void Present(in AudioVolumeComponent volume, AudioVolumeViewContext context)
        {
            if (context.mixer == null)
            {
                ReportFailure(context, "Audio Mixer가 없어 볼륨을 반영하지 못했다.");
                return;
            }

            bool isMusicApplied = context.mixer.SetFloat(MUSIC_VOLUME_PARAMETER, ToDecibels(volume.musicVolume));
            bool isSoundEffectApplied = context.mixer.SetFloat(SOUND_EFFECT_VOLUME_PARAMETER, ToDecibels(volume.soundEffectVolume));
            if (!isMusicApplied || !isSoundEffectApplied)
            {
                string missingParameters = isMusicApplied ? SOUND_EFFECT_VOLUME_PARAMETER : isSoundEffectApplied ? MUSIC_VOLUME_PARAMETER : MUSIC_VOLUME_PARAMETER + ", " + SOUND_EFFECT_VOLUME_PARAMETER;
                ReportFailure(context, $"Audio Mixer {context.mixer.name}에 노출된 파라미터 {missingParameters}가 없어 볼륨을 반영하지 못했다.");
            }
        }

        private static void ReportFailure(AudioVolumeViewContext context, string message)
        {
            if (context.hasReportedFailure)
            {
                return;
            }

            Debug.LogWarning($"AudioVolumeViewSystem >> {message}");
            context.hasReportedFailure = true;
        }

        // 사람의 귀는 소리 크기를 로그로 느끼므로, 선형 볼륨을 데시벨로 바꿔야 슬라이더가 고르게 들린다
        private static float ToDecibels(float volume)
        {
            if (volume <= 0f)
            {
                return MINIMUM_DECIBELS;
            }

            return Mathf.Max(MINIMUM_DECIBELS, 20f * Mathf.Log10(volume));
        }
    }
}
