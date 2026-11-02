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
        /// <param name="masks">칸별 컴포넌트 표시</param>
        /// <param name="actionStates">플레이어 행동 상태와 공격 진행</param>
        public static void Tick(ComponentMask[] masks, PlayerActionStateComponent[] actionStates)
        {
            for (int index = 0; index < masks.Length; index++)
            {
                if ((masks[index] & ComponentMask.PlayerArchetype) != ComponentMask.PlayerArchetype)
                {
                    continue;
                }

                ref PlayerActionStateComponent actionState = ref actionStates[index];
                actionState.elapsedTicks++;
                if (actionState.attackCooldownTicks > 0)
                {
                    actionState.attackCooldownTicks--;
                }
            }
        }
    }
}
