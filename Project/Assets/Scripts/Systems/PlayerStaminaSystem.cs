using TopView.Components;
using TopView.Entities;
using UnityEngine;

namespace TopView.Systems
{
    // 플레이어 마나를 회복한다.
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

        private static void Recover(ref ManaComponent mana, in PlayerVitalsComponent vital)
        {
            if (mana.recoveryElapsedTicks < vital.manaRecoveryDelayTicks)
            {
                mana.recoveryElapsedTicks++;
            }

            if (mana.current < mana.max && mana.recoveryElapsedTicks >= vital.manaRecoveryDelayTicks)
            {
                mana.current = Mathf.Min(mana.current + vital.manaRecoveryPerTick, mana.max);
            }
        }
    }
}
