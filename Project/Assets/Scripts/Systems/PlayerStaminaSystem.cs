using TopView.Components;
using TopView.Entities;
using UnityEngine;

namespace TopView.Systems
{
    // 플레이어 마나의 소모와 회복을 처리한다.
    public static class PlayerStaminaSystem
    {
        /// <summary>
        /// 마나 회복 지연 시간을 1틱 늘리고, 지연 시간이 지났으면 마나를 회복한다.
        /// </summary>
        /// <param name="players">플레이어 저장소</param>
        public static void Tick(PlayerStorage players)
        {
            for (int index = 0; index < players.entities.Length; index++)
            {
                ref PlayerEntity entity = ref players.entities[index];
                if (entity.isAlive)
                {
                    Recover(ref entity.mana, in entity.vitals);
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

        private static void Recover(ref ManaComponent mana, in PlayerVitalsComponent vital)
        {
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
}
