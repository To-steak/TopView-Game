using TopView.Commands;
using TopView.Components;
using TopView.Entities;
using TopView.Utilities;
using UnityEngine;

namespace TopView.Systems
{
    // 움직일 수 있는 상태의 플레이어에게 이동 입력을 이동 방향과 속도로 반영한다.
    public static class PlayerMoveInputSystem
    {
        /// <summary>
        /// Move, 공격, 교체, 재장전, Jump, Land 상태는 방향과 속도를 정하고 달리기 마나를 쓴다. PlayerPressSystem 다음에 부른다.
        /// </summary>
        /// <param name="players">플레이어 저장소</param>
        /// <param name="command">플레이어 입력</param>
        public static void Tick(PlayerStorage players, in PlayerCommand command)
        {
            for (int index = 0; index < players.entities.Length; index++)
            {
                ref PlayerEntity entity = ref players.entities[index];
                if (!entity.isAlive)
                {
                    continue;
                }

                switch (entity.actionState.state)
                {
                    case PlayerState.Move:
                    case PlayerState.Shot:
                    case PlayerState.Swing:
                    case PlayerState.ThrowCharge:
                    case PlayerState.Throw:
                    case PlayerState.Swap:
                    case PlayerState.Reload:
                    case PlayerState.Jump:
                    case PlayerState.Land:
                        ApplyMoveInput(ref entity.motion, ref entity.mana, in entity.movement, in entity.vitals, in entity.upgrade, in entity.upgradeLevels, in command);
                        break;
                }
            }
        }

        // 입력이 없으면 서 있고, 달리기 마나도 쓰지 않는다.
        private static void ApplyMoveInput(ref PlayerMotionComponent motion, ref ManaComponent mana, in PlayerMovementComponent movement, in PlayerVitalsComponent vital, in PlayerUpgradeComponent upgrade, in PlayerUpgradeLevelsComponent upgradeLevels, in PlayerCommand command)
        {
            if (command.moveDirection == Vector3.zero)
            {
                motion.moveDirection = Vector3.zero;
                return;
            }

            bool isRunning = command.isRunning;
            if (isRunning)
            {
                float requiredMana = PlayerUpgradeUtility.GetRunManaCost(in vital, in upgrade, in upgradeLevels);
                if (PlayerStaminaUtility.HasEnoughMana(in mana, requiredMana))
                {
                    PlayerStaminaUtility.UseMana(ref mana, requiredMana);
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
