using TopView.Commands;
using TopView.Components;
using UnityEngine;

namespace TopView.Utilities
{
    // 설정 명령의 볼륨 값을 볼륨에 넣는 도구. 설정 화면이 있는 씬(Menu, World)이 함께 쓴다.
    public static class AudioVolumeUtility
    {
        /// <summary>
        /// 볼륨 명령(SetMusicVolume, SetSoundEffectVolume)의 값을 0~1로 맞춰 볼륨에 넣는다. 다른 명령이면 볼륨을 바꾸지 않는다.
        /// </summary>
        /// <param name="volume">바꿀 볼륨</param>
        /// <param name="command">볼륨 명령</param>
        /// <returns>명령의 값이 0~1 안이었으면 true. 범위 밖이라 맞췄으면 false</returns>
        public static bool Apply(ref AudioVolumeComponent volume, in SettingsCommand command)
        {
            float clampedVolume = Mathf.Clamp01(command.value);
            switch (command.type)
            {
                case SettingsCommandType.SetMusicVolume:
                    volume.musicVolume = clampedVolume;
                    break;
                case SettingsCommandType.SetSoundEffectVolume:
                    volume.soundEffectVolume = clampedVolume;
                    break;
                default:
                    Debug.LogWarning($"AudioVolumeUtility >> 볼륨 명령이 아닌 {command.type} 명령이 들어와 볼륨을 바꾸지 않았다.");
                    return true;
            }

            return clampedVolume == command.value;
        }
    }
}
