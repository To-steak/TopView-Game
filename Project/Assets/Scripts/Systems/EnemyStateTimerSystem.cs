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
        /// <param name="enemies">적 저장소</param>
        public static void Tick(EnemyStorage enemies)
        {
            for (int index = 0; index < enemies.entities.Length; index++)
            {
                ref EnemyEntity entity = ref enemies.entities[index];
                if (entity.isAlive)
                {
                    Advance(ref entity.actionState);
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
