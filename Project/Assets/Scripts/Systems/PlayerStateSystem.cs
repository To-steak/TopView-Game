using System.Collections.Generic;
using TopView.Commands;
using TopView.Components;
using TopView.Entities;
using TopView.Requests;
using TopView.Utilities;
using UnityEngine;

namespace TopView.Systems
{
    // 이동 입력과 상태 시간을 보고 플레이어의 다음 행동 상태를 요청한다. 실제 전환은 PlayerStateChangeSystem이 한다.
    public static class PlayerStateSystem
    {
        // 처리할 플레이어가 가진 컴포넌트
        private const ComponentMask PLAYER_MASK = ComponentMask.PlayerActionState | ComponentMask.Death | ComponentMask.PlayerAction | ComponentMask.PlayerMotion | ComponentMask.WeaponSlots;

        /// <summary>
        /// 상태별로 다음 상태로 넘어갈 때인지 보고, 교체와 재장전을 끝낸다. 사망한 뒤 시간이 되면 게임오버를 요청한다. PlayerAttackSystem 다음, PlayerStateChangeSystem 앞에 부른다.
        /// </summary>
        /// <param name="storage">엔티티 저장소. 플레이어(PlayerActionState, Death, PlayerAction, PlayerMotion, WeaponSlots)만 처리한다</param>
        /// <param name="weapons">무기 저장소</param>
        /// <param name="command">플레이어 입력</param>
        /// <param name="stateChangeRequests">플레이어 상태 변경 요청 큐</param>
        /// <param name="gameOverRequests">게임오버 요청 큐</param>
        public static void Tick(EntityStorage storage, WeaponStorage weapons, in PlayerCommand command, Queue<PlayerStateChangeRequest> stateChangeRequests, Queue<GameOverRequest> gameOverRequests)
        {
            for (int index = 0; index < storage.entities.Length; index++)
            {
                ref Entity entity = ref storage.entities[index];
                if ((entity.mask & PLAYER_MASK) != PLAYER_MASK)
                {
                    continue;
                }

                ref PlayerActionStateComponent actionState = ref entity.playerActionState;
                ref readonly PlayerMotionComponent motion = ref entity.playerMotion;
                ref PlayerWeaponSlotsComponent weaponSlot = ref entity.weaponSlots;
                ref readonly PlayerActionComponent action = ref entity.playerAction;
                ref readonly DeathComponent death = ref entity.death;
                ref WeaponEntity weaponEntity = ref weapons.entities[weaponSlot.weapons[weaponSlot.currentIndex].index];
                ref readonly WeaponComponent weapon = ref weaponEntity.weapon;
                EntityIdentifier identifier = new EntityIdentifier { index = index, generation = entity.generation };
                Vector3 moveInput = command.moveDirection;

                PlayerState nextState = moveInput == Vector3.zero ? PlayerState.Idle : PlayerState.Move;
                bool isFinished = false;

                switch (actionState.state)
                {
                    case PlayerState.Idle:
                    case PlayerState.Move:
                        isFinished = actionState.state != nextState;
                        break;

                    case PlayerState.Jump:
                        if (motion.verticalVelocity <= 0f && motion.isGrounded)
                        {
                            stateChangeRequests.Enqueue(new PlayerStateChangeRequest { target = identifier, state = PlayerState.Land, moveInput = moveInput });
                        }
                        break;

                    case PlayerState.Land:
                        isFinished = actionState.elapsedTicks >= action.landTicks;
                        break;

                    case PlayerState.Dodge:
                        isFinished = actionState.elapsedTicks >= action.dodgeTicks;
                        break;

                    case PlayerState.ThrowCharge:
                        // 취소하면 탄약을 쓰지 않고 Idle이나 Move로, 공격 키를 떼면 그때의 충전량으로 Throw에 들어간다
                        float chargePerTick = weapon.throwing.chargePerTick;
                        if (actionState.elapsedTicks == 0 && chargePerTick <= 0f)
                        {
                            Debug.LogWarning($"PlayerStateSystem >> chargePerTick({chargePerTick})이 0 이하라 투척 충전량이 0에 머문다. chargePerTick은 0보다 커야 한다.");
                        }
                        actionState.throwCharge = PlayerWeaponUtility.GetThrowCharge(actionState.elapsedTicks, chargePerTick);
                        if (command.isCancelPressed)
                        {
                            isFinished = true;
                        }
                        else if (!command.isAttackHeld)
                        {
                            stateChangeRequests.Enqueue(new PlayerStateChangeRequest { target = identifier, state = PlayerState.Throw, moveInput = moveInput });
                        }
                        break;

                    case PlayerState.Shot:
                    case PlayerState.Swing:
                    case PlayerState.Throw:
                        isFinished = PlayerWeaponUtility.IsAttackFinished(in actionState, in weapon);
                        break;

                    case PlayerState.Swap:
                        PlayerWeaponUtility.CommitSwap(ref actionState, ref weaponSlot, in action);
                        isFinished = actionState.elapsedTicks >= action.swapTicks;
                        break;

                    case PlayerState.Reload:
                        // 취소하면 탄약을 채우지 않고 Idle이나 Move로, 재장전 시간이 다 차면 탄약을 채우고 끝난다
                        if (command.isCancelPressed)
                        {
                            isFinished = true;
                            break;
                        }
                        isFinished = actionState.elapsedTicks >= action.reloadTicks;
                        if (isFinished)
                        {
                            PlayerWeaponUtility.Reload(in weapon, ref weaponEntity.ammoState, weaponEntity.upgradeLevel.level);
                        }
                        break;

                    case PlayerState.Die:
                        if (death.isGameOverOnDeath && actionState.elapsedTicks == death.dieTicks)
                        {
                            gameOverRequests.Enqueue(new GameOverRequest { isVictory = false });
                        }
                        break;
                }

                // 행동이 끝났거나 이동 입력이 바뀌었으면 입력에 따라 Idle이나 Move로 간다
                if (isFinished)
                {
                    stateChangeRequests.Enqueue(new PlayerStateChangeRequest { target = identifier, state = nextState, moveInput = moveInput });
                }
            }
        }
    }
}
