using TopView.Components;
using UnityEngine;

namespace TopView.Persistence
{
    // 볼륨을 PlayerPrefs에 저장하고 불러온다.
    public static class AudioVolumeStore
    {
        private const string MUSIC_VOLUME_KEY = "Audio.MusicVolume";
        private const string SOUND_EFFECT_VOLUME_KEY = "Audio.SoundEffectVolume";

        /// <summary>
        /// 저장된 볼륨을 불러온다. 저장된 값이 없으면 설정값의 시작값을 쓴다.
        /// </summary>
        /// <param name="settings">볼륨 설정값</param>
        /// <returns>불러온 볼륨</returns>
        public static AudioVolumeComponent Load(in AudioSettingsComponent settings)
        {
            return new AudioVolumeComponent
            {
                musicVolume = LoadVolume(MUSIC_VOLUME_KEY, settings.startingMusicVolume),
                soundEffectVolume = LoadVolume(SOUND_EFFECT_VOLUME_KEY, settings.startingSoundEffectVolume)
            };
        }

        /// <summary>
        /// 볼륨을 저장하고 디스크에 쓴다.
        /// </summary>
        /// <param name="volume">저장할 볼륨</param>
        public static void Save(in AudioVolumeComponent volume)
        {
            PlayerPrefs.SetFloat(MUSIC_VOLUME_KEY, volume.musicVolume);
            PlayerPrefs.SetFloat(SOUND_EFFECT_VOLUME_KEY, volume.soundEffectVolume);
            PlayerPrefs.Save();
        }

        private static float LoadVolume(string key, float startingVolume)
        {
            if (!PlayerPrefs.HasKey(key))
            {
                return startingVolume;
            }

            float volume = PlayerPrefs.GetFloat(key);
            if (volume < 0f || volume > 1f)
            {
                Debug.LogWarning($"AudioVolumeStore >> 저장된 {key} 값 {volume}이 0~1 밖이라 범위 안으로 맞춘다.");
                return Mathf.Clamp01(volume);
            }

            return volume;
        }
    }
}
