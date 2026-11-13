using TopView.Components;
using TopView.Entities;
using UnityEngine;

namespace TopView.Systems
{
    // 플레이어 마나를 회복한다.
    public static class PlayerStaminaSystem
    {
        // 처리할 플레이어가 가진 컴포넌트
        private const ComponentMask PLAYER_MASK = ComponentMask.PlayerActionState | ComponentMask.PlayerVitals | ComponentMask.Mana;

        /// <summary>
        /// 마나 회복 지연 시간을 1틱 늘리고, 지연 시간이 지났으면 마나를 회복한다.
        /// </summary>
        /// <param name="storage">엔티티 저장소. 플레이어(PlayerActionState, PlayerVitals, Mana)만 처리한다</param>
        public static void Tick(EntityStorage storage)
        {
            for (int index = 0; index < storage.entities.Length; index++)
            {
                ref Entity entity = ref storage.entities[index];
                if ((entity.mask & PLAYER_MASK) == PLAYER_MASK)
                {
                    Recover(ref entity.mana, in entity.playerVitals);
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
