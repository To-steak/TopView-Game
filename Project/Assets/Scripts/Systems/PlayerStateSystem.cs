using System.Collections.Generic;
using TopView.Commands;
using TopView.Components;
using TopView.Entities;
using TopView.Records;
using TopView.Requests;
using UnityEngine;

namespace TopView.Systems
{
    // 플레이어의 행동 상태를 이동 입력과 상태 시간으로 바꾸고, 상태에 들어가고 나갈 때의 값을 정한다.
    public static class PlayerStateSystem
    {
        /// <summary>
        /// 상태별로 다음 상태로 넘어갈 때인지 보고, 교체와 재장전을 끝낸다. 사망한 뒤 시간이 되면 게임오버를 요청한다. PlayerAttackSystem 다음에 부른다.
        /// </summary>
        /// <param name="players">플레이어 저장소</param>
        /// <param name="command">플레이어 입력</param>
        /// <param name="stateRecords">상태 변경 기록</param>
        /// <param name="gameOverRequests">게임오버 요청 큐</param>
        public static void Tick(PlayerStorage players, in PlayerCommand command, List<PlayerStateChangedRecord> stateRecords, Queue<GameOverRequest> gameOverRequests)
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
                ref readonly PlayerActionComponent action = ref entity.action;
                WeaponComponent[] weapons = entity.weaponList.weapons;
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
                            ChangeState(ref actionState, ref motion, ref rotation, ref mana, in weaponSlot, in movement, in vital, weapons, in identifier, PlayerState.Land, moveInput, stateRecords);
                        }
                        break;

                    case PlayerState.Land:
                        isFinished = actionState.elapsedTicks >= action.landTicks;
                        break;

                    case PlayerState.Dodge:
                        isFinished = actionState.elapsedTicks >= action.dodgeTicks;
                        break;

                    case PlayerState.Shot:
                    case PlayerState.Swing:
                    case PlayerState.Throw:
                        isFinished = PlayerWeaponSystem.IsAttackFinished(in actionState, in weapons[weaponSlot.currentIndex]);
                        break;

                    case PlayerState.Swap:
                        PlayerWeaponSystem.CommitSwap(ref actionState, ref weaponSlot, in action);
                        isFinished = actionState.elapsedTicks >= action.swapTicks;
                        break;

                    case PlayerState.Reload:
                        isFinished = actionState.elapsedTicks >= action.reloadTicks;
                        if (isFinished)
                        {
                            PlayerWeaponSystem.Reload(weapons, ref weaponSlot);
                        }
                        break;

                    case PlayerState.Die:
                        if (actionState.elapsedTicks == action.dieTicks)
                        {
                            gameOverRequests.Enqueue(new GameOverRequest());
                        }
                        break;
                }

                // 행동이 끝났거나 이동 입력이 바뀌었으면 입력에 따라 Idle이나 Move로 간다
                if (isFinished)
                {
                    ChangeState(ref actionState, ref motion, ref rotation, ref mana, in weaponSlot, in movement, in vital, weapons, in identifier, nextState, moveInput, stateRecords);
                }
            }
        }

        /// <summary>
        /// 지금 상태에서 나가고 새 상태에 들어가 상태 변경을 기록한다. Die 상태에서는 바꾸지 않는다.
        /// </summary>
        /// <param name="actionState">행동 상태와 공격 진행</param>
        /// <param name="motion">이동 상태</param>
        /// <param name="rotation">회전. 회피 방향으로 돈다</param>
        /// <param name="mana">마나. 점프와 회피에 쓴다</param>
        /// <param name="weaponSlot">든 무기와 무기별 탄약</param>
        /// <param name="movement">플레이어 이동 값</param>
        /// <param name="vital">플레이어 체력과 마나 값</param>
        /// <param name="weapons">무기 목록</param>
        /// <param name="identifier">플레이어를 가리키는 값</param>
        /// <param name="newState">새 상태</param>
        /// <param name="moveInput">이동 입력. 회피 방향에 쓴다</param>
        /// <param name="stateRecords">상태 변경 기록</param>
        public static void ChangeState(ref PlayerActionStateComponent actionState, ref PlayerMotionComponent motion, ref RotationComponent rotation, ref ManaComponent mana, in PlayerWeaponSlotsComponent weaponSlot, in PlayerMovementComponent movement, in PlayerVitalsComponent vital, WeaponComponent[] weapons, in EntityIdentifier identifier, PlayerState newState, Vector3 moveInput, List<PlayerStateChangedRecord> stateRecords)
        {
            if (actionState.state == PlayerState.Die)
            {
                return;
            }

            ExitState(in actionState, ref motion);

            stateRecords.Add(new PlayerStateChangedRecord
            {
                player = identifier,
                previousState = actionState.state,
                state = newState
            });

            actionState.state = newState;
            actionState.elapsedTicks = 0;
            actionState.isAttackCommitted = false;

            EnterState(ref actionState, ref motion, ref rotation, ref mana, in weaponSlot, in movement, in vital, weapons, moveInput);
        }

        private static void EnterState(ref PlayerActionStateComponent actionState, ref PlayerMotionComponent motion, ref RotationComponent rotation, ref ManaComponent mana, in PlayerWeaponSlotsComponent weaponSlot, in PlayerMovementComponent movement, in PlayerVitalsComponent vital, WeaponComponent[] weapons, Vector3 moveInput)
        {
            switch (actionState.state)
            {
                case PlayerState.Idle:
                    motion.moveDirection = Vector3.zero;
                    break;

                case PlayerState.Jump:
                    motion.verticalVelocity = movement.jumpPower;
                    PlayerStaminaSystem.UseMana(ref mana, vital.jumpManaCost);
                    motion.currentSpeed = movement.inAirSpeed;
                    break;

                case PlayerState.Dodge:
                    motion.isRotationLocked = true;
                    // 이동 입력이 있으면 그쪽으로, 서 있으면 바라보는 쪽(XZ)으로 회피한다
                    Vector3 dodgeDirection = moveInput;
                    if (dodgeDirection == Vector3.zero)
                    {
                        dodgeDirection = rotation.current * Vector3.forward;
                        dodgeDirection.y = 0f;
                        dodgeDirection.Normalize();
                    }
                    motion.moveDirection = dodgeDirection;
                    motion.currentSpeed = movement.dodgeSpeed;
                    if (motion.moveDirection == Vector3.zero)
                    {
                        Debug.LogWarning("PlayerStateSystem >> 회피 방향을 정하지 못해 회피 방향으로 돌지 않았다.");
                    }
                    else
                    {
                        rotation.current = Quaternion.LookRotation(motion.moveDirection);
                    }
                    PlayerStaminaSystem.UseMana(ref mana, vital.dodgeManaCost);
                    break;

                case PlayerState.Shot:
                    actionState.attackCooldownTicks = weapons[weaponSlot.currentIndex].attackIntervalTicks;
                    break;

                case PlayerState.Swing:
                    motion.isRotationLocked = true;
                    actionState.attackCooldownTicks = weapons[weaponSlot.currentIndex].attackIntervalTicks;
                    break;

                case PlayerState.Throw:
                    motion.isRotationLocked = true;
                    break;
            }
        }

        private static void ExitState(in PlayerActionStateComponent actionState, ref PlayerMotionComponent motion)
        {
            switch (actionState.state)
            {
                case PlayerState.Dodge:
                case PlayerState.Swing:
                case PlayerState.Throw:
                    motion.isRotationLocked = false;
                    break;
            }
        }
    }
}
