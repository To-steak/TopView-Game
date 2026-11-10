using System.Collections.Generic;
using TopView.Commands;
using TopView.Components;
using TopView.Entities;
using TopView.Requests;
using TopView.Utilities;

namespace TopView.Systems
{
    // 누름 입력을 지금 상태에 맞게 처리해 플레이어의 상태 변경을 요청한다. 점프, 회피, 교체, 재장전, 공격 순으로 보고, 요청을 하나 넣으면 나머지는 보지 않는다.
    public static class PlayerPressSystem
    {
        /// <summary>
        /// Idle이나 Move인 플레이어에게 누름 입력을 반영한다. PlayerStateChangeSystem 앞에 부른다.
        /// </summary>
        /// <param name="players">플레이어 저장소</param>
        /// <param name="command">플레이어 입력</param>
        /// <param name="stateChangeRequests">플레이어 상태 변경 요청 큐</param>
        public static void Tick(PlayerStorage players, in PlayerCommand command, Queue<PlayerStateChangeRequest> stateChangeRequests)
        {
            for (int index = 0; index < players.entities.Length; index++)
            {
                ref PlayerEntity entity = ref players.entities[index];
                if (!entity.isAlive || !IsIdleOrMove(entity.actionState.state))
                {
                    continue;
                }

                ref readonly PlayerActionStateComponent actionState = ref entity.actionState;
                ref readonly PlayerMotionComponent motion = ref entity.motion;
                ref readonly ManaComponent mana = ref entity.mana;
                ref PlayerWeaponSlotsComponent weaponSlot = ref entity.weaponSlots;
                ref readonly PlayerVitalsComponent vital = ref entity.vitals;
                WeaponComponent[] weapons = entity.weaponList.weapons;
                EntityIdentifier identifier = new EntityIdentifier { index = index, generation = entity.generation };

                if (command.isJumpPressed && motion.isGrounded && PlayerStaminaUtility.HasEnoughMana(in mana, PlayerUpgradeUtility.GetJumpManaCost(in vital, in entity.upgrade, in entity.upgradeLevels)))
                {
                    stateChangeRequests.Enqueue(new PlayerStateChangeRequest { target = identifier, state = PlayerState.Jump, moveInput = command.moveDirection });
                    continue;
                }

                if (command.isDodgePressed && motion.isGrounded && PlayerStaminaUtility.HasEnoughMana(in mana, PlayerUpgradeUtility.GetDodgeManaCost(in vital, in entity.upgrade, in entity.upgradeLevels)))
                {
                    stateChangeRequests.Enqueue(new PlayerStateChangeRequest { target = identifier, state = PlayerState.Dodge, moveInput = command.moveDirection });
                    continue;
                }

                if (command.swapWeaponIndex >= 0 && PlayerWeaponUtility.CanSwap(weapons, in weaponSlot, command.swapWeaponIndex))
                {
                    weaponSlot.pendingIndex = command.swapWeaponIndex;
                    stateChangeRequests.Enqueue(new PlayerStateChangeRequest { target = identifier, state = PlayerState.Swap, moveInput = command.moveDirection });
                    continue;
                }

                if (command.isReloadPressed && PlayerWeaponUtility.CanReload(weapons, in weaponSlot, in entity.upgradeLevels))
                {
                    stateChangeRequests.Enqueue(new PlayerStateChangeRequest { target = identifier, state = PlayerState.Reload, moveInput = command.moveDirection });
                    continue;
                }

                // 공격 키를 누르고 있어도 다른 입력이 사라지지 않도록 공격을 마지막에 본다
                if ((command.isAttackPressed || command.isAttackHeld) && motion.isGrounded && actionState.attackCooldownTicks == 0)
                {
                    // 투척 충전은 던진 뒤에도 누르고 있으면 다시 충전되지 않도록 누른 입력으로만 들어간다
                    if (PlayerWeaponUtility.TryGetAttackState(weapons, in weaponSlot, out PlayerState attackState) && (command.isAttackPressed || attackState != PlayerState.ThrowCharge))
                    {
                        stateChangeRequests.Enqueue(new PlayerStateChangeRequest { target = identifier, state = attackState, moveInput = command.moveDirection });
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
