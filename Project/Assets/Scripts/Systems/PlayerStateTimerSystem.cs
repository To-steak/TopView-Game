using TopView.Components;
using TopView.Entities;

namespace TopView.Systems
{
    // 플레이어의 상태 시간과 공격 대기 시간을 센다.
    public static class PlayerStateTimerSystem
    {
        /// <summary>
        /// 상태 시간을 1틱 늘리고 공격 대기 시간을 1틱 줄인다.
        /// </summary>
        /// <param name="players">플레이어 저장소</param>
        public static void Tick(PlayerStorage players)
        {
            for (int index = 0; index < players.entities.Length; index++)
            {
                ref PlayerEntity entity = ref players.entities[index];
                if (entity.isAlive)
                {
                    Advance(ref entity.actionState);
                }
            }
        }

        private static void Advance(ref PlayerActionStateComponent actionState)
        {
            actionState.elapsedTicks++;
            if (actionState.attackCooldownTicks > 0)
            {
                actionState.attackCooldownTicks--;
            }
        }
    }
}
