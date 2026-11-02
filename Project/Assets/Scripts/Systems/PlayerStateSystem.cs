using System.Collections.Generic;
using TopView.Commands;
using TopView.Components;
using TopView.Entities;
using TopView.Records;
using TopView.Requests;
using UnityEngine;

namespace TopView.Systems
{
    // 플레이어의 행동 상태를 입력과 상태 타이머로 바꾸고, 상태에 들어가고 나갈 때의 값을 정한다. 무기 처리는 PlayerWeaponSystem이 한다.
    public static class PlayerStateSystem
    {
        /// <summary>
        /// 체력이 0이면 사망 상태로 바꾼다. 누름 입력을 지금 상태에 맞게 처리한 뒤, 상태별 갱신과 타이머(공격 발동, 상태 종료)를 처리한다.
        /// </summary>
        /// <param name="masks">칸별 컴포넌트 표시</param>
        /// <param name="generations">칸이 채워진 횟수</param>
        /// <param name="healths">체력</param>
        /// <param name="positions">위치</param>
        /// <param name="rotations">회전</param>
        /// <param name="manas">마나</param>
        /// <param name="movements">플레이어 이동 값</param>
        /// <param name="vitals">플레이어 체력과 마나 값</param>
        /// <param name="actions">플레이어 행동 상태의 길이</param>
        /// <param name="actionStates">플레이어 행동 상태와 공격 진행</param>
        /// <param name="motions">플레이어 이동 상태</param>
        /// <param name="weaponLists">무기 목록</param>
        /// <param name="weaponSlots">든 무기와 무기별 탄약</param>
        /// <param name="melees">근접 판정 값과 진행 시간. 사망할 때 자기 근접 판정을 없앤다</param>
        /// <param name="freeMeleeIndices">근접 판정 구간의 빈 칸 번호 목록</param>
        /// <param name="command">플레이어 입력</param>
        /// <param name="stateRecords">상태 변경 기록</param>
        /// <param name="projectileSpawnRequests">투사체 생성 요청 큐</param>
        /// <param name="meleeSpawnRequests">근접 판정 생성 요청 큐</param>
        /// <param name="gameOverRequests">게임오버 요청 큐</param>
        public static void Tick(ComponentMask[] masks, int[] generations, HealthComponent[] healths, PositionComponent[] positions, RotationComponent[] rotations, ManaComponent[] manas, PlayerMovementComponent[] movements, PlayerVitalsComponent[] vitals, PlayerActionComponent[] actions, PlayerActionStateComponent[] actionStates, PlayerMotionComponent[] motions, WeaponListComponent[] weaponLists, PlayerWeaponSlotsComponent[] weaponSlots, MeleeComponent[] melees, List<int> freeMeleeIndices, in PlayerCommand command, List<PlayerStateChangedRecord> stateRecords, Queue<ProjectileSpawnRequest> projectileSpawnRequests, Queue<MeleeSpawnRequest> meleeSpawnRequests, Queue<GameOverRequest> gameOverRequests)
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
                if (healths[index].current <= 0f && actionState.state != PlayerState.Die)
                {
                    ChangeState(ref actionState, ref motion, ref rotation, ref mana, in weaponSlot, in movement, in vital, weapons, in identifier, PlayerState.Die, Vector3.zero, stateRecords);
                    MeleeHitSystem.DespawnOwnedBy(masks, melees, freeMeleeIndices, in identifier);
                }

                actionState.elapsedTicks++;
                if (actionState.attackCooldownTicks > 0)
                {
                    actionState.attackCooldownTicks--;
                }

                HandlePresses(ref actionState, ref motion, ref rotation, ref mana, ref weaponSlot, in movement, in vital, weapons, in identifier, in command, stateRecords);
                UpdateState(ref actionState, ref motion, ref rotation, ref mana, ref weaponSlot, in movement, in vital, in actions[index], weapons, positions[index].current, in identifier, in command, stateRecords, projectileSpawnRequests, meleeSpawnRequests, gameOverRequests);
            }
        }

        // 지금 상태에서 나가고 새 상태에 들어간다. Die 상태에서는 바꾸지 않는다. moveInput은 회피 방향에 쓴다.
        private static void ChangeState(ref PlayerActionStateComponent actionState, ref PlayerMotionComponent motion, ref RotationComponent rotation, ref ManaComponent mana, in PlayerWeaponSlotsComponent weaponSlot, in PlayerMovementComponent movement, in PlayerVitalsComponent vital, WeaponComponent[] weapons, in EntityIdentifier identifier, PlayerState newState, Vector3 moveInput, List<PlayerStateChangedRecord> stateRecords)
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

        private static void HandlePresses(ref PlayerActionStateComponent actionState, ref PlayerMotionComponent motion, ref RotationComponent rotation, ref ManaComponent mana, ref PlayerWeaponSlotsComponent weaponSlot, in PlayerMovementComponent movement, in PlayerVitalsComponent vital, WeaponComponent[] weapons, in EntityIdentifier identifier, in PlayerCommand command, List<PlayerStateChangedRecord> stateRecords)
        {
            if (command.isJumpPressed && IsIdleOrMove(actionState.state))
            {
                if (motion.isGrounded && PlayerStaminaSystem.HasEnoughMana(in mana, vital.jumpManaCost))
                {
                    ChangeState(ref actionState, ref motion, ref rotation, ref mana, in weaponSlot, in movement, in vital, weapons, in identifier, PlayerState.Jump, command.moveDirection, stateRecords);
                }
            }

            if (command.isDodgePressed && IsIdleOrMove(actionState.state))
            {
                if (motion.isGrounded && PlayerStaminaSystem.HasEnoughMana(in mana, vital.dodgeManaCost))
                {
                    ChangeState(ref actionState, ref motion, ref rotation, ref mana, in weaponSlot, in movement, in vital, weapons, in identifier, PlayerState.Dodge, command.moveDirection, stateRecords);
                }
            }

            if (command.swapWeaponIndex >= 0 && IsIdleOrMove(actionState.state))
            {
                if (PlayerWeaponSystem.CanSwap(weapons, in weaponSlot, command.swapWeaponIndex))
                {
                    weaponSlot.pendingIndex = command.swapWeaponIndex;
                    ChangeState(ref actionState, ref motion, ref rotation, ref mana, in weaponSlot, in movement, in vital, weapons, in identifier, PlayerState.Swap, command.moveDirection, stateRecords);
                }
            }

            if (command.isReloadPressed && IsIdleOrMove(actionState.state))
            {
                if (PlayerWeaponSystem.CanReload(weapons, in weaponSlot))
                {
                    ChangeState(ref actionState, ref motion, ref rotation, ref mana, in weaponSlot, in movement, in vital, weapons, in identifier, PlayerState.Reload, command.moveDirection, stateRecords);
                }
            }

            // 공격 키를 누르고 있어도 다른 입력이 사라지지 않도록 공격을 마지막에 본다
            if ((command.isAttackPressed || command.isAttackHeld) && IsIdleOrMove(actionState.state) && motion.isGrounded && actionState.attackCooldownTicks == 0)
            {
                // Throw는 누르고 있을 때 탄약을 다 쓰지 않도록 누른 입력으로만 나간다
                if (PlayerWeaponSystem.TryGetAttackState(weapons, in weaponSlot, out PlayerState attackState) && (command.isAttackPressed || attackState != PlayerState.Throw))
                {
                    ChangeState(ref actionState, ref motion, ref rotation, ref mana, in weaponSlot, in movement, in vital, weapons, in identifier, attackState, command.moveDirection, stateRecords);
                }
            }
        }

        private static void UpdateState(ref PlayerActionStateComponent actionState, ref PlayerMotionComponent motion, ref RotationComponent rotation, ref ManaComponent mana, ref PlayerWeaponSlotsComponent weaponSlot, in PlayerMovementComponent movement, in PlayerVitalsComponent vital, in PlayerActionComponent action, WeaponComponent[] weapons, Vector3 position, in EntityIdentifier identifier, in PlayerCommand command, List<PlayerStateChangedRecord> stateRecords, Queue<ProjectileSpawnRequest> projectileSpawnRequests, Queue<MeleeSpawnRequest> meleeSpawnRequests, Queue<GameOverRequest> gameOverRequests)
        {
            switch (actionState.state)
            {
                case PlayerState.Idle:
                    if (!IsZero(command.moveDirection))
                    {
                        ChangeState(ref actionState, ref motion, ref rotation, ref mana, in weaponSlot, in movement, in vital, weapons, in identifier, PlayerState.Move, command.moveDirection, stateRecords);
                    }
                    break;

                case PlayerState.Move:
                    if (IsZero(command.moveDirection))
                    {
                        ChangeState(ref actionState, ref motion, ref rotation, ref mana, in weaponSlot, in movement, in vital, weapons, in identifier, PlayerState.Idle, command.moveDirection, stateRecords);
                        break;
                    }
                    ApplyMoveInput(ref motion, ref mana, in movement, in vital, in command);
                    break;

                case PlayerState.Jump:
                    motion.moveDirection = command.moveDirection;
                    if (motion.verticalVelocity <= 0f && motion.isGrounded)
                    {
                        ChangeState(ref actionState, ref motion, ref rotation, ref mana, in weaponSlot, in movement, in vital, weapons, in identifier, PlayerState.Land, command.moveDirection, stateRecords);
                    }
                    break;

                case PlayerState.Land:
                    motion.moveDirection = command.moveDirection;
                    if (actionState.elapsedTicks >= action.landTicks)
                    {
                        FinishAction(ref actionState, ref motion, ref rotation, ref mana, in weaponSlot, in movement, in vital, weapons, in identifier, command.moveDirection, stateRecords);
                    }
                    break;

                case PlayerState.Dodge:
                    if (actionState.elapsedTicks >= action.dodgeTicks)
                    {
                        FinishAction(ref actionState, ref motion, ref rotation, ref mana, in weaponSlot, in movement, in vital, weapons, in identifier, command.moveDirection, stateRecords);
                    }
                    break;

                case PlayerState.Shot:
                case PlayerState.Swing:
                case PlayerState.Throw:
                    ApplyMoveInput(ref motion, ref mana, in movement, in vital, in command);
                    UpdateAttack(ref actionState, ref motion, ref rotation, ref mana, ref weaponSlot, in movement, in vital, weapons, position, in identifier, command.moveDirection, stateRecords, projectileSpawnRequests, meleeSpawnRequests);
                    break;

                case PlayerState.Swap:
                    ApplyMoveInput(ref motion, ref mana, in movement, in vital, in command);
                    PlayerWeaponSystem.CommitSwap(ref actionState, ref weaponSlot, in action);
                    if (actionState.elapsedTicks >= action.swapTicks)
                    {
                        FinishAction(ref actionState, ref motion, ref rotation, ref mana, in weaponSlot, in movement, in vital, weapons, in identifier, command.moveDirection, stateRecords);
                    }
                    break;

                case PlayerState.Reload:
                    ApplyMoveInput(ref motion, ref mana, in movement, in vital, in command);
                    if (actionState.elapsedTicks >= action.reloadTicks)
                    {
                        PlayerWeaponSystem.Reload(weapons, ref weaponSlot);
                        FinishAction(ref actionState, ref motion, ref rotation, ref mana, in weaponSlot, in movement, in vital, weapons, in identifier, command.moveDirection, stateRecords);
                    }
                    break;

                case PlayerState.Die:
                    if (actionState.elapsedTicks == action.dieTicks)
                    {
                        gameOverRequests.Enqueue(new GameOverRequest());
                    }
                    break;
            }
        }

        // 이동 입력을 이동 방향과 속도에 반영한다. 입력이 없으면 서 있고, 달리기 마나도 쓰지 않는다.
        private static void ApplyMoveInput(ref PlayerMotionComponent motion, ref ManaComponent mana, in PlayerMovementComponent movement, in PlayerVitalsComponent vital, in PlayerCommand command)
        {
            if (IsZero(command.moveDirection))
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

        private static void UpdateAttack(ref PlayerActionStateComponent actionState, ref PlayerMotionComponent motion, ref RotationComponent rotation, ref ManaComponent mana, ref PlayerWeaponSlotsComponent weaponSlot, in PlayerMovementComponent movement, in PlayerVitalsComponent vital, WeaponComponent[] weapons, Vector3 position, in EntityIdentifier identifier, Vector3 moveInput, List<PlayerStateChangedRecord> stateRecords, Queue<ProjectileSpawnRequest> projectileSpawnRequests, Queue<MeleeSpawnRequest> meleeSpawnRequests)
        {
            ref readonly WeaponComponent weapon = ref weapons[weaponSlot.currentIndex];

            if (!actionState.isAttackCommitted && actionState.elapsedTicks >= weapon.attackStartTicks)
            {
                PlayerWeaponSystem.CommitAttack(ref actionState, ref weaponSlot, in movement, position, rotation.current, in identifier, in weapon, projectileSpawnRequests, meleeSpawnRequests);
            }

            if (PlayerWeaponSystem.IsAttackFinished(in actionState, in weapon))
            {
                FinishAction(ref actionState, ref motion, ref rotation, ref mana, in weaponSlot, in movement, in vital, weapons, in identifier, moveInput, stateRecords);
            }
        }

        // 행동이 끝났을 때 이동 입력에 따라 Idle이나 Move로 돌아간다.
        private static void FinishAction(ref PlayerActionStateComponent actionState, ref PlayerMotionComponent motion, ref RotationComponent rotation, ref ManaComponent mana, in PlayerWeaponSlotsComponent weaponSlot, in PlayerMovementComponent movement, in PlayerVitalsComponent vital, WeaponComponent[] weapons, in EntityIdentifier identifier, Vector3 moveInput, List<PlayerStateChangedRecord> stateRecords)
        {
            PlayerState nextState = IsZero(moveInput) ? PlayerState.Idle : PlayerState.Move;
            ChangeState(ref actionState, ref motion, ref rotation, ref mana, in weaponSlot, in movement, in vital, weapons, in identifier, nextState, moveInput, stateRecords);
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
                    if (IsZero(dodgeDirection))
                    {
                        dodgeDirection = rotation.current * Vector3.forward;
                        dodgeDirection.y = 0f;
                        dodgeDirection.Normalize();
                    }
                    motion.moveDirection = dodgeDirection;
                    motion.currentSpeed = movement.dodgeSpeed;
                    if (IsZero(motion.moveDirection))
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

                case PlayerState.Die:
                    motion.moveDirection = Vector3.zero;
                    motion.isRotationLocked = true;
                    break;
            }
        }

        private static void ExitState(in PlayerActionStateComponent actionState, ref PlayerMotionComponent motion)
        {
            switch (actionState.state)
            {
                case PlayerState.Dodge:
                    motion.isRotationLocked = false;
                    break;

                case PlayerState.Swing:
                case PlayerState.Throw:
                    motion.isRotationLocked = false;
                    break;
            }
        }

        private static bool IsIdleOrMove(PlayerState state)
        {
            return state == PlayerState.Idle || state == PlayerState.Move;
        }

        private static bool IsZero(Vector3 direction)
        {
            return direction == Vector3.zero;
        }
    }
}
