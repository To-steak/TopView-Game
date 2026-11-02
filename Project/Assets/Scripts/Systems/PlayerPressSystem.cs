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
        /// <param name="masks">칸별 컴포넌트 표시</param>
        /// <param name="generations">칸이 채워진 횟수</param>
        /// <param name="rotations">회전</param>
        /// <param name="manas">마나</param>
        /// <param name="movements">플레이어 이동 값</param>
        /// <param name="vitals">플레이어 체력과 마나 값</param>
        /// <param name="actionStates">플레이어 행동 상태와 공격 진행</param>
        /// <param name="motions">플레이어 이동 상태</param>
        /// <param name="weaponLists">무기 목록</param>
        /// <param name="weaponSlots">든 무기와 무기별 탄약</param>
        /// <param name="command">플레이어 입력</param>
        /// <param name="stateRecords">상태 변경 기록</param>
        public static void Tick(ComponentMask[] masks, int[] generations, RotationComponent[] rotations, ManaComponent[] manas, PlayerMovementComponent[] movements, PlayerVitalsComponent[] vitals, PlayerActionStateComponent[] actionStates, PlayerMotionComponent[] motions, WeaponListComponent[] weaponLists, PlayerWeaponSlotsComponent[] weaponSlots, in PlayerCommand command, List<PlayerStateChangedRecord> stateRecords)
        {
            for (int index = 0; index < masks.Length; index++)
            {
                if ((masks[index] & ComponentMask.PlayerArchetype) != ComponentMask.PlayerArchetype)
                {
                    continue;
                }

                ref PlayerActionStateComponent actionState = ref actionStates[index];
                ref PlayerMotionComponent motion = ref motions[index];
                ref RotationComponent rotation = ref rotations[index];
                ref ManaComponent mana = ref manas[index];
                ref PlayerWeaponSlotsComponent weaponSlot = ref weaponSlots[index];
                ref readonly PlayerMovementComponent movement = ref movements[index];
                ref readonly PlayerVitalsComponent vital = ref vitals[index];
                WeaponComponent[] weapons = weaponLists[index].weapons;
                EntityIdentifier identifier = new EntityIdentifier { index = index, generation = generations[index] };

                if (command.isJumpPressed && IsIdleOrMove(actionState.state))
                {
                    if (motion.isGrounded && PlayerStaminaSystem.HasEnoughMana(in mana, vital.jumpManaCost))
                    {
                        PlayerStateSystem.ChangeState(ref actionState, ref motion, ref rotation, ref mana, in weaponSlot, in movement, in vital, weapons, in identifier, PlayerState.Jump, command.moveDirection, stateRecords);
                    }
                }

                if (command.isDodgePressed && IsIdleOrMove(actionState.state))
                {
                    if (motion.isGrounded && PlayerStaminaSystem.HasEnoughMana(in mana, vital.dodgeManaCost))
                    {
                        PlayerStateSystem.ChangeState(ref actionState, ref motion, ref rotation, ref mana, in weaponSlot, in movement, in vital, weapons, in identifier, PlayerState.Dodge, command.moveDirection, stateRecords);
                    }
                }

                if (command.swapWeaponIndex >= 0 && IsIdleOrMove(actionState.state))
                {
                    if (PlayerWeaponSystem.CanSwap(weapons, in weaponSlot, command.swapWeaponIndex))
                    {
                        weaponSlot.pendingIndex = command.swapWeaponIndex;
                        PlayerStateSystem.ChangeState(ref actionState, ref motion, ref rotation, ref mana, in weaponSlot, in movement, in vital, weapons, in identifier, PlayerState.Swap, command.moveDirection, stateRecords);
                    }
                }

                if (command.isReloadPressed && IsIdleOrMove(actionState.state))
                {
                    if (PlayerWeaponSystem.CanReload(weapons, in weaponSlot))
                    {
                        PlayerStateSystem.ChangeState(ref actionState, ref motion, ref rotation, ref mana, in weaponSlot, in movement, in vital, weapons, in identifier, PlayerState.Reload, command.moveDirection, stateRecords);
                    }
                }

                // 공격 키를 누르고 있어도 다른 입력이 사라지지 않도록 공격을 마지막에 본다
                if ((command.isAttackPressed || command.isAttackHeld) && IsIdleOrMove(actionState.state) && motion.isGrounded && actionState.attackCooldownTicks == 0)
                {
                    // Throw는 누르고 있을 때 탄약을 다 쓰지 않도록 누른 입력으로만 나간다
                    if (PlayerWeaponSystem.TryGetAttackState(weapons, in weaponSlot, out PlayerState attackState) && (command.isAttackPressed || attackState != PlayerState.Throw))
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
