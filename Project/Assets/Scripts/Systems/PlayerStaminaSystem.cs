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
            for (int playerIndex = 0; playerIndex < players.entities.Length; playerIndex++)
            {
                ref PlayerEntity player = ref players.entities[playerIndex];
                if (!player.isAlive)
                {
                    continue;
                }

                if (player.manaRecoveryElapsedTicks < player.vitals.manaRecoveryDelayTicks)
                {
                    player.manaRecoveryElapsedTicks++;
                }

                if (player.mana < player.vitals.maxMana && player.manaRecoveryElapsedTicks >= player.vitals.manaRecoveryDelayTicks)
                {
                    player.mana = Mathf.Min(player.mana + player.vitals.manaRecoveryPerTick, player.vitals.maxMana);
                }
            }
        }

        /// <summary>
        /// 마나가 충분한지 돌려준다.
        /// </summary>
        /// <param name="player">플레이어</param>
        /// <param name="amount">필요한 마나</param>
        /// <returns>마나가 amount 이상이면 true</returns>
        public static bool HasEnoughMana(in PlayerEntity player, float amount)
        {
            return player.mana >= amount;
        }

        /// <summary>
        /// 마나를 쓰고 회복 지연 시간을 처음부터 다시 센다. 마나는 0 아래로 내려가지 않는다.
        /// </summary>
        /// <param name="player">플레이어</param>
        /// <param name="amount">쓸 마나</param>
        public static void UseMana(ref PlayerEntity player, float amount)
        {
            player.mana = Mathf.Max(player.mana - amount, 0f);
            player.manaRecoveryElapsedTicks = 0;
        }
    }
}
