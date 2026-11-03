using System.Collections.Generic;
using TopView.Commands;
using TopView.Components;
using TopView.Entities;
using TopView.Records;

namespace TopView.Systems
{
    // 누름 입력을 지금 상태에 맞게 처리해 플레이어를 행동 상태로 보낸다. 점프, 회피, 교체, 재장전, 공격 순으로 본다.
    public static class PlayerPressSystem
    {
        /// <summary>
        /// Idle이나 Move인 플레이어에게 누름 입력을 반영한다.
        /// </summary>
        /// <param name="players">플레이어 저장소</param>
        /// <param name="command">플레이어 입력</param>
        /// <param name="stateRecords">상태 변경 기록</param>
        public static void Tick(PlayerStorage players, in PlayerCommand command, List<PlayerStateChangedRecord> stateRecords)
        {
            for (int index = 0; index < players.entities.Length; index++)
            {
                ref PlayerEntity entity = ref players.entities[index];
                if (!entity.isAlive)
                {
                    continue;
                }

                ref PlayerActionStateComponent actionState = ref entity.actionState;
                ref PlayerMotionComponent motion = ref entity.motion;
                ref RotationComponent rotation = ref entity.rotation;
                ref ManaComponent mana = ref entity.mana;
                ref PlayerWeaponSlotsComponent weaponSlot = ref entity.weaponSlots;
                ref readonly PlayerMovementComponent movement = ref entity.movement;
                ref readonly PlayerVitalsComponent vital = ref entity.vitals;
                WeaponComponent[] weapons = entity.weaponList.weapons;
                EntityIdentifier identifier = new EntityIdentifier { index = index, generation = entity.generation };

                if (command.isJumpPressed && IsIdleOrMove(actionState.state))
                {
                    if (motion.isGrounded && PlayerStaminaUtility.HasEnoughMana(in mana, vital.jumpManaCost))
                    {
                        PlayerStateSystem.ChangeState(ref actionState, ref motion, ref rotation, ref mana, in weaponSlot, in movement, in vital, weapons, in identifier, PlayerState.Jump, command.moveDirection, stateRecords);
                    }
                }

                if (command.isDodgePressed && IsIdleOrMove(actionState.state))
                {
                    if (motion.isGrounded && PlayerStaminaUtility.HasEnoughMana(in mana, vital.dodgeManaCost))
                    {
                        PlayerStateSystem.ChangeState(ref actionState, ref motion, ref rotation, ref mana, in weaponSlot, in movement, in vital, weapons, in identifier, PlayerState.Dodge, command.moveDirection, stateRecords);
                    }
                }

                if (command.swapWeaponIndex >= 0 && IsIdleOrMove(actionState.state))
                {
                    if (PlayerWeaponUtility.CanSwap(weapons, in weaponSlot, command.swapWeaponIndex))
                    {
                        weaponSlot.pendingIndex = command.swapWeaponIndex;
                        PlayerStateSystem.ChangeState(ref actionState, ref motion, ref rotation, ref mana, in weaponSlot, in movement, in vital, weapons, in identifier, PlayerState.Swap, command.moveDirection, stateRecords);
                    }
                }

                if (command.isReloadPressed && IsIdleOrMove(actionState.state))
                {
                    if (PlayerWeaponUtility.CanReload(weapons, in weaponSlot))
                    {
                        PlayerStateSystem.ChangeState(ref actionState, ref motion, ref rotation, ref mana, in weaponSlot, in movement, in vital, weapons, in identifier, PlayerState.Reload, command.moveDirection, stateRecords);
                    }
                }

                // 공격 키를 누르고 있어도 다른 입력이 사라지지 않도록 공격을 마지막에 본다
                if ((command.isAttackPressed || command.isAttackHeld) && IsIdleOrMove(actionState.state) && motion.isGrounded && actionState.attackCooldownTicks == 0)
                {
                    // Throw는 누르고 있을 때 탄약을 다 쓰지 않도록 누른 입력으로만 나간다
                    if (PlayerWeaponUtility.TryGetAttackState(weapons, in weaponSlot, out PlayerState attackState) && (command.isAttackPressed || attackState != PlayerState.Throw))
                    {
                        PlayerStateSystem.ChangeState(ref actionState, ref motion, ref rotation, ref mana, in weaponSlot, in movement, in vital, weapons, in identifier, attackState, command.moveDirection, stateRecords);
                    }
                }
            }
        }

        private static bool IsIdleOrMove(PlayerState state)
        {
            return state == PlayerState.Idle || state == PlayerState.Move;
        }
    }
}
