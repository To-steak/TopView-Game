using TopView.Components;
using TopView.Entities;

namespace TopView.Systems
{
    // 적의 상태 시간과 공격별 대기 시간을 센다.
    public static class EnemyStateTimerSystem
    {
        /// <summary>
        /// 상태 시간을 1틱 늘리고 공격별 대기 시간을 1틱 줄인다.
        /// </summary>
        /// <param name="masks">칸별 컴포넌트 표시</param>
        /// <param name="actionStates">적 행동 상태와 공격 진행</param>
        public static void Tick(ComponentMask[] masks, EnemyActionStateComponent[] actionStates)
        {
            for (int index = 0; index < masks.Length; index++)
            {
                if ((masks[index] & ComponentMask.EnemyArchetype) != ComponentMask.EnemyArchetype)
                {
                    continue;
                }

                ref EnemyActionStateComponent actionState = ref actionStates[index];
                actionState.elapsedTicks++;

                int[] cooldowns = actionState.attackCooldownRemainingTicks;
                for (int attackIndex = 0; attackIndex < cooldowns.Length; attackIndex++)
                {
                    if (cooldowns[attackIndex] > 0)
                    {
                        cooldowns[attackIndex]--;
                    }
                }
            }
        }
    }
}
