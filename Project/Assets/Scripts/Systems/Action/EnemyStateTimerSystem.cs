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
        /// <param name="storage">엔티티 저장소. EnemyActionState를 가진 칸의 상태 시간과 대기 시간을 고친다</param>
        public static void Tick(EntityStorage storage)
        {
            for (int index = 0; index < storage.entities.Length; index++)
            {
                ref Entity entity = ref storage.entities[index];
                if ((entity.mask & ComponentMask.EnemyActionState) != 0)
                {
                    Advance(ref entity.enemyActionState);
                }
            }
        }

        private static void Advance(ref EnemyActionStateComponent actionState)
        {
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
