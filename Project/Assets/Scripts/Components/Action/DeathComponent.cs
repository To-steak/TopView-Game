using System;

namespace TopView.Components
{
    // 사망 상태의 길이와 사라질 때 일어나는 일
    [Serializable]
    public struct DeathComponent
    {
        // 사망 상태에 들어간 뒤 사라지기까지의 시간(틱)
        public int dieTicks;
        // 사망 상태가 끝날 때 게임을 끝내는지. 플레이어와 보스에 켠다. 플레이어면 패배, 적이면 승리로 끝난다
        public bool isGameOverOnDeath;
    }
}
