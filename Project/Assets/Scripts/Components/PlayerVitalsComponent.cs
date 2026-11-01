using System;

namespace TopView.Components
{
    // 플레이어의 체력과 마나 값
    [Serializable]
    public struct PlayerVitalsComponent
    {
        // 최대 체력
        public float maxHealth;
        // 최대 마나
        public float maxMana;
        // 마나 회복량(틱당)
        public float manaRecoveryPerTick;
        // 마나를 쓴 뒤 회복이 시작되기까지의 시간(틱)
        public int manaRecoveryDelayTicks;
        // 달리는 동안의 마나 소모량(틱당)
        public float runManaCostPerTick;
        // 회피 한 번의 마나 소모량
        public float dodgeManaCost;
        // 점프 한 번의 마나 소모량
        public float jumpManaCost;
    }
}
