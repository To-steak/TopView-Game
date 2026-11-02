using TopView.Commands;
using TopView.Components;
using TopView.Entities;
using UnityEngine;

namespace TopView.Systems
{
    // 움직일 수 있는 상태의 플레이어에게 이동 입력을 이동 방향과 속도로 반영한다.
    public static class PlayerMoveInputSystem
    {
        /// <summary>
        /// Move, 공격, 교체, 재장전 상태는 방향과 속도를 정하고 달리기 마나를 쓴다. Jump와 Land 상태는 방향만 정한다. PlayerPressSystem 다음에 부른다.
        /// </summary>
        /// <param name="masks">칸별 컴포넌트 표시</param>
        /// <param name="actionStates">플레이어 행동 상태</param>
        /// <param name="motions">플레이어 이동 상태</param>
        /// <param name="manas">마나</param>
        /// <param name="movements">플레이어 이동 값</param>
        /// <param name="vitals">플레이어 체력과 마나 값</param>
        /// <param name="command">플레이어 입력</param>
        public static void Tick(ComponentMask[] masks, PlayerActionStateComponent[] actionStates, PlayerMotionComponent[] motions, ManaComponent[] manas, PlayerMovementComponent[] movements, PlayerVitalsComponent[] vitals, in PlayerCommand command)
        {
            for (int index = 0; index < masks.Length; index++)
            {
                if ((masks[index] & ComponentMask.PlayerArchetype) != ComponentMask.PlayerArchetype)
                {
                    continue;
                }

                switch (actionStates[index].state)
                {
                    case PlayerState.Move:
                    case PlayerState.Shot:
                    case PlayerState.Swing:
                    case PlayerState.Throw:
                    case PlayerState.Swap:
                    case PlayerState.Reload:
                        ApplyMoveInput(ref motions[index], ref manas[index], in movements[index], in vitals[index], in command);
                        break;

                    case PlayerState.Jump:
                    case PlayerState.Land:
                        motions[index].moveDirection = command.moveDirection;
                        break;
                }
            }
        }

        // 입력이 없으면 서 있고, 달리기 마나도 쓰지 않는다.
        private static void ApplyMoveInput(ref PlayerMotionComponent motion, ref ManaComponent mana, in PlayerMovementComponent movement, in PlayerVitalsComponent vital, in PlayerCommand command)
        {
            if (command.moveDirection == Vector3.zero)
            {
                motion.moveDirection = Vector3.zero;
                return;
            }

            bool isRunning = command.isRunning;
            if (isRunning)
            {
                float requiredMana = vital.runManaCostPerTick;
                if (PlayerStaminaSystem.HasEnoughMana(in mana, requiredMana))
                {
                    PlayerStaminaSystem.UseMana(ref mana, requiredMana);
                }
                else
                {
                    isRunning = false;
                }
            }

            motion.moveDirection = command.moveDirection;
            motion.currentSpeed = isRunning ? movement.runSpeed : movement.walkSpeed;
            motion.isRunning = isRunning;
        }
    }
}
