using TopView.Components;
using UnityEngine;

namespace TopView.Utilities
{
    // 플레이어 마나의 확인과 소모 도구.
    public static class PlayerManaUtility
    {
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
