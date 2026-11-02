using TopView.Components;
using TopView.Entities;
using UnityEngine;

namespace TopView.Systems
{
    // 플레이어 마나의 소모와 회복을 처리한다.
    public static class PlayerStaminaSystem
    {
        // 마나가 회복되는 엔티티가 가지는 컴포넌트
        private const ComponentMask RECOVERY_QUERY = ComponentMask.Mana | ComponentMask.PlayerVitals;

        /// <summary>
        /// 마나 회복 지연 시간을 1틱 늘리고, 지연 시간이 지났으면 마나를 회복한다.
        /// </summary>
        /// <param name="masks">칸별 컴포넌트 표시</param>
        /// <param name="manas">마나</param>
        /// <param name="vitals">플레이어 체력과 마나 값</param>
        public static void Tick(ComponentMask[] masks, ManaComponent[] manas, PlayerVitalsComponent[] vitals)
        {
            for (int index = 0; index < masks.Length; index++)
            {
                if ((masks[index] & RECOVERY_QUERY) != RECOVERY_QUERY)
                {
                    continue;
                }

                ref ManaComponent mana = ref manas[index];
                ref readonly PlayerVitalsComponent vital = ref vitals[index];

                if (mana.recoveryElapsedTicks < vital.manaRecoveryDelayTicks)
                {
                    mana.recoveryElapsedTicks++;
                }

                if (mana.current < vital.maxMana && mana.recoveryElapsedTicks >= vital.manaRecoveryDelayTicks)
                {
                    mana.current = Mathf.Min(mana.current + vital.manaRecoveryPerTick, vital.maxMana);
                }
            }
        }

        /// <summary>
        /// 마나가 충분한지 돌려준다.
        /// </summary>
        /// <param name="mana">마나</param>
        /// <param name="amount">필요한 마나</param>
        /// <returns>마나가 amount 이상이면 true</returns>
        public static bool HasEnoughMana(in ManaComponent mana, float amount)
        {
            return mana.current >= amount;
        }

        /// <summary>
        /// 마나를 쓰고 회복 지연 시간을 처음부터 다시 센다. 마나는 0 아래로 내려가지 않는다.
        /// </summary>
        /// <param name="mana">마나</param>
        /// <param name="amount">쓸 마나</param>
        public static void UseMana(ref ManaComponent mana, float amount)
        {
            mana.current = Mathf.Max(mana.current - amount, 0f);
            mana.recoveryElapsedTicks = 0;
        }
    }
}
