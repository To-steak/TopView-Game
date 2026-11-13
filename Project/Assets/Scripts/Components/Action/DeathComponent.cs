using System;
using UnityEngine.Serialization;

namespace TopView.Components
{
    // 사망 상태의 길이와 사라질 때 일어나는 일
    [Serializable]
    public struct DeathComponent
    {
        // 사망 상태에 들어간 뒤 사라지기까지의 시간(틱)
        public int dieTicks;
        // 사망 상태가 끝나 사라질 때 게임을 끝내는지. 보스에 켠다
        [FormerlySerializedAs("endsGameOnDeath")]
        public bool isGameOverOnDeath;
    }
}
