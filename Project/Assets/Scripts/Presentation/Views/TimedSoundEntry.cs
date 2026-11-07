using System;
using UnityEngine;

namespace TopView.Presentation.Views
{
    // 행동이 시작된 뒤 정해진 틱에 재생할 소리 하나
    [Serializable]
    public struct TimedSoundEntry
    {
        // 재생할 Clip
        public AudioClip clip;
        // 행동 상태에 들어간 뒤 재생하기까지의 시간(틱). 0이면 들어간 틱에 재생한다
        public int startTicks;
    }
}
