using System;
using UnityEngine;

namespace TopView.Components
{
    // 볼륨 설정값. 저장된 볼륨이 없을 때의 시작값을 담는다.
    [Serializable]
    public struct AudioSettingsComponent
    {
        // 저장된 값이 없을 때의 배경음악 볼륨(0~1)
        [Range(0f, 1f)]
        public float startingMusicVolume;
        // 저장된 값이 없을 때의 효과음 볼륨(0~1)
        [Range(0f, 1f)]
        public float startingSoundEffectVolume;
    }
}
