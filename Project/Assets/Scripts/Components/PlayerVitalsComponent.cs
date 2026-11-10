using System;
using UnityEngine.Serialization;

namespace TopView.Components
{
    // 플레이어의 체력과 마나 값
    [Serializable]
    public struct PlayerVitalsComponent
    {
        // 시작할 때의 최대 체력. 게임 중의 최대 체력은 HealthComponent.max이며 강화로 오른다
        [FormerlySerializedAs("maxHealth")]
        public float startingMaxHealth;
        // 시작할 때의 최대 마나. 게임 중의 최대 마나는 ManaComponent.max이며 강화로 오른다
        [FormerlySerializedAs("maxMana")]
        public float startingMaxMana;
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
