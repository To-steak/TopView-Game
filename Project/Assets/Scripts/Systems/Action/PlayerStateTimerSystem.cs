using TopView.Components;
using TopView.Entities;

namespace TopView.Systems
{
    // 플레이어의 상태 시간과 공격 대기 시간을 센다.
    public static class PlayerStateTimerSystem
    {
        // 처리할 플레이어가 가진 컴포넌트
        private const ComponentMask PLAYER_MASK = ComponentMask.PlayerActionState;

        /// <summary>
        /// 상태 시간을 1틱 늘리고 공격 대기 시간을 1틱 줄인다.
        /// </summary>
        /// <param name="storage">엔티티 저장소. 플레이어(PlayerActionState)만 처리한다</param>
        public static void Tick(EntityStorage storage)
        {
            for (int index = 0; index < storage.entities.Length; index++)
            {
                ref Entity entity = ref storage.entities[index];
                if ((entity.mask & PLAYER_MASK) == PLAYER_MASK)
                {
                    Advance(ref entity.playerActionState);
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
