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
        /// <param name="players">플레이어 저장소</param>
        /// <param name="command">플레이어 입력</param>
        /// <param name="stateRecords">상태 변경 기록</param>
        /// <param name="projectileSpawnRequests">투사체 생성 요청 큐</param>
        /// <param name="meleeSpawnRequests">근접 판정 생성 요청 큐</param>
        /// <param name="masks">공유 저장소의 칸별 컴포넌트 표시. 사망할 때 자기 근접 판정을 없앤다</param>
        /// <param name="melees">근접 판정 값과 진행 시간</param>
        /// <param name="freeMeleeIndices">근접 판정 구간의 빈 칸 번호 목록</param>
        /// <param name="gameOverRequests">게임오버 요청 큐</param>
        public static void Tick(PlayerStorage players, in PlayerCommand command, List<PlayerStateChangedRecord> stateRecords, Queue<ProjectileSpawnRequest> projectileSpawnRequests, Queue<MeleeSpawnRequest> meleeSpawnRequests, ComponentMask[] masks, MeleeComponent[] melees, List<int> freeMeleeIndices, Queue<GameOverRequest> gameOverRequests)
        {
            for (int playerIndex = 0; playerIndex < players.entities.Length; playerIndex++)
            {
                ref PlayerEntity player = ref players.entities[playerIndex];
                if (!player.isAlive)
                {
                    continue;
                }

                EntityIdentifier identifier = new EntityIdentifier { index = playerIndex, generation = player.generation };
                if (player.health.current <= 0f && player.actionState.state != PlayerState.Die)
                {
                    ChangeState(ref player, in identifier, PlayerState.Die, Vector3.zero, stateRecords);
                    MeleeHitSystem.DespawnOwnedBy(masks, melees, freeMeleeIndices, new EntityReference { kind = EntityKind.Player, identifier = identifier });
                }

                player.actionState.elapsedTicks++;
                if (player.actionState.attackCooldownTicks > 0)
                {
                    player.actionState.attackCooldownTicks--;
                }

                HandlePresses(ref player, in identifier, in command, stateRecords);
                UpdateState(ref player, in identifier, in command, stateRecords, projectileSpawnRequests, meleeSpawnRequests, gameOverRequests);
            }
        }

        // 지금 상태에서 나가고 새 상태에 들어간다. Die 상태에서는 바꾸지 않는다. moveInput은 회피 방향에 쓴다.
        private static void ChangeState(ref PlayerEntity player, in EntityIdentifier identifier, PlayerState newState, Vector3 moveInput, List<PlayerStateChangedRecord> stateRecords)
        {
            if (player.actionState.state == PlayerState.Die)
            {
                return;
            }

            ExitState(ref player);

            stateRecords.Add(new PlayerStateChangedRecord
            {
                player = identifier,
                previousState = player.actionState.state,
                state = newState
            });

            player.actionState.state = newState;
            player.actionState.elapsedTicks = 0;
            player.actionState.isAttackCommitted = false;

            EnterState(ref player, moveInput);
        }

        private static void HandlePresses(ref PlayerEntity player, in EntityIdentifier identifier, in PlayerCommand command, List<PlayerStateChangedRecord> stateRecords)
        {
            if (command.isJumpPressed && IsIdleOrMove(player.actionState.state))
            {
                if (player.motion.isGrounded && PlayerStaminaSystem.HasEnoughMana(in player, player.vitals.jumpManaCost))
                {
                    ChangeState(ref player, in identifier, PlayerState.Jump, command.moveDirection, stateRecords);
                }
            }

            if (command.isDodgePressed && IsIdleOrMove(player.actionState.state))
            {
                if (player.motion.isGrounded && PlayerStaminaSystem.HasEnoughMana(in player, player.vitals.dodgeManaCost))
                {
                    ChangeState(ref player, in identifier, PlayerState.Dodge, command.moveDirection, stateRecords);
                }
            }

            if (command.swapWeaponIndex >= 0 && IsIdleOrMove(player.actionState.state))
            {
                if (PlayerWeaponSystem.CanSwap(in player, command.swapWeaponIndex))
                {
                    player.weaponSlots.pendingIndex = command.swapWeaponIndex;
                    ChangeState(ref player, in identifier, PlayerState.Swap, command.moveDirection, stateRecords);
                }
            }

            if (command.isReloadPressed && IsIdleOrMove(player.actionState.state))
            {
                if (PlayerWeaponSystem.CanReload(in player))
                {
                    ChangeState(ref player, in identifier, PlayerState.Reload, command.moveDirection, stateRecords);
                }
            }

            // 공격 키를 누르고 있어도 다른 입력이 사라지지 않도록 공격을 마지막에 본다
            if ((command.isAttackPressed || command.isAttackHeld) && IsIdleOrMove(player.actionState.state) && player.motion.isGrounded && player.actionState.attackCooldownTicks == 0)
            {
                // Throw는 누르고 있을 때 탄약을 다 쓰지 않도록 누른 입력으로만 나간다
                if (PlayerWeaponSystem.TryGetAttackState(in player, out PlayerState attackState) &&(command.isAttackPressed || attackState != PlayerState.Throw))
                {
                    ChangeState(ref player, in identifier, attackState, command.moveDirection, stateRecords);
                }
            }
        }

        private static void UpdateState(ref PlayerEntity player, in EntityIdentifier identifier, in PlayerCommand command, List<PlayerStateChangedRecord> stateRecords, Queue<ProjectileSpawnRequest> projectileSpawnRequests, Queue<MeleeSpawnRequest> meleeSpawnRequests, Queue<GameOverRequest> gameOverRequests)
        {
            switch (player.actionState.state)
            {
                case PlayerState.Idle:
                    if (!IsZero(command.moveDirection))
                    {
                        ChangeState(ref player, in identifier, PlayerState.Move, command.moveDirection, stateRecords);
                    }
                    break;

                case PlayerState.Move:
                    UpdateMove(ref player, in identifier, in command, stateRecords);
                    break;

                case PlayerState.Jump:
                    player.motion.moveDirection = command.moveDirection;
                    if (player.motion.verticalVelocity <= 0f && player.motion.isGrounded)
                    {
                        ChangeState(ref player, in identifier, PlayerState.Land, command.moveDirection, stateRecords);
                    }
                    break;

                case PlayerState.Land:
                    player.motion.moveDirection = command.moveDirection;
                    if (player.actionState.elapsedTicks >= player.action.landTicks)
                    {
                        FinishAction(ref player, in identifier, command.moveDirection, stateRecords);
                    }
                    break;

                case PlayerState.Dodge:
                    if (player.actionState.elapsedTicks >= player.action.dodgeTicks)
                    {
                        FinishAction(ref player, in identifier, command.moveDirection, stateRecords);
                    }
                    break;

                case PlayerState.Shot:
                case PlayerState.Swing:
                case PlayerState.Throw:
                    ApplyMoveInput(ref player, in command);
                    UpdateAttack(ref player, in identifier, command.moveDirection, stateRecords, projectileSpawnRequests, meleeSpawnRequests);
                    break;

                case PlayerState.Swap:
                    ApplyMoveInput(ref player, in command);
                    PlayerWeaponSystem.CommitSwap(ref player);
                    if (player.actionState.elapsedTicks >= player.action.swapTicks)
                    {
                        FinishAction(ref player, in identifier, command.moveDirection, stateRecords);
                    }
                    break;

                case PlayerState.Reload:
                    ApplyMoveInput(ref player, in command);
                    if (player.actionState.elapsedTicks >= player.action.reloadTicks)
                    {
                        PlayerWeaponSystem.Reload(ref player);
                        FinishAction(ref player, in identifier, command.moveDirection, stateRecords);
                    }
                    break;

                case PlayerState.Die:
                    if (player.actionState.elapsedTicks == player.action.dieTicks)
                    {
                        gameOverRequests.Enqueue(new GameOverRequest());
                    }
                    break;
            }
        }

        private static void UpdateMove(ref PlayerEntity player, in EntityIdentifier identifier, in PlayerCommand command, List<PlayerStateChangedRecord> stateRecords)
        {
            if (IsZero(command.moveDirection))
            {
                ChangeState(ref player, in identifier, PlayerState.Idle, command.moveDirection, stateRecords);
                return;
            }

            ApplyMoveInput(ref player, in command);
        }

        // 이동 입력을 이동 방향과 속도에 반영한다. 입력이 없으면 서 있고, 달리기 마나도 쓰지 않는다.
        private static void ApplyMoveInput(ref PlayerEntity player, in PlayerCommand command)
        {
            if (IsZero(command.moveDirection))
            {
                player.motion.moveDirection = Vector3.zero;
                return;
            }

            bool isRunning = command.isRunning;
            if (isRunning)
            {
                float requiredMana = player.vitals.runManaCostPerTick;
                if (PlayerStaminaSystem.HasEnoughMana(in player, requiredMana))
                {
                    PlayerStaminaSystem.UseMana(ref player, requiredMana);
                }
                else
                {
                    isRunning = false;
                }
            }

            player.motion.moveDirection = command.moveDirection;
            player.motion.currentSpeed = isRunning ? player.movement.runSpeed : player.movement.walkSpeed;
            player.motion.isRunning = isRunning;
        }

        private static void UpdateAttack(ref PlayerEntity player, in EntityIdentifier identifier, Vector3 moveInput, List<PlayerStateChangedRecord> stateRecords, Queue<ProjectileSpawnRequest> projectileSpawnRequests, Queue<MeleeSpawnRequest> meleeSpawnRequests)
        {
            ref readonly WeaponComponent weapon = ref player.weapons[player.weaponSlots.currentIndex];

            if (!player.actionState.isAttackCommitted && player.actionState.elapsedTicks >= weapon.attackStartTicks)
            {
                PlayerWeaponSystem.CommitAttack(ref player, in identifier, in weapon, projectileSpawnRequests, meleeSpawnRequests);
            }

            if (PlayerWeaponSystem.IsAttackFinished(in player, in weapon))
            {
                FinishAction(ref player, in identifier, moveInput, stateRecords);
            }
        }

        // 행동이 끝났을 때 이동 입력에 따라 Idle이나 Move로 돌아간다.
        private static void FinishAction(ref PlayerEntity player, in EntityIdentifier identifier, Vector3 moveInput, List<PlayerStateChangedRecord> stateRecords)
        {
            PlayerState nextState = IsZero(moveInput) ? PlayerState.Idle : PlayerState.Move;
            ChangeState(ref player, in identifier, nextState, moveInput, stateRecords);
        }

        private static void EnterState(ref PlayerEntity player, Vector3 moveInput)
        {
            switch (player.actionState.state)
            {
                case PlayerState.Idle:
                    player.motion.moveDirection = Vector3.zero;
                    break;

                case PlayerState.Jump:
                    player.motion.verticalVelocity = player.movement.jumpPower;
                    PlayerStaminaSystem.UseMana(ref player, player.vitals.jumpManaCost);
                    player.motion.currentSpeed = player.movement.inAirSpeed;
                    break;

                case PlayerState.Dodge:
                    player.motion.isRotationLocked = true;
                    // 이동 입력이 있으면 그쪽으로, 서 있으면 바라보는 쪽(XZ)으로 회피한다
                    Vector3 dodgeDirection = moveInput;
                    if (IsZero(dodgeDirection))
                    {
                        dodgeDirection = player.rotation.current * Vector3.forward;
                        dodgeDirection.y = 0f;
                        dodgeDirection.Normalize();
                    }
                    player.motion.moveDirection = dodgeDirection;
                    player.motion.currentSpeed = player.movement.dodgeSpeed;
                    if (IsZero(player.motion.moveDirection))
                    {
                        Debug.LogWarning("PlayerStateSystem >> 회피 방향을 정하지 못해 회피 방향으로 돌지 않았다.");
                    }
                    else
                    {
                        player.rotation.current = Quaternion.LookRotation(player.motion.moveDirection);
                    }
                    PlayerStaminaSystem.UseMana(ref player, player.vitals.dodgeManaCost);
                    break;

                case PlayerState.Shot:
                    player.actionState.attackCooldownTicks = player.weapons[player.weaponSlots.currentIndex].attackIntervalTicks;
                    break;

                case PlayerState.Swing:
                    player.motion.isRotationLocked = true;
                    player.actionState.attackCooldownTicks = player.weapons[player.weaponSlots.currentIndex].attackIntervalTicks;
                    break;

                case PlayerState.Throw:
                    player.motion.isRotationLocked = true;
                    break;

                case PlayerState.Die:
                    player.motion.moveDirection = Vector3.zero;
                    player.motion.isRotationLocked = true;
                    break;
            }
        }

        private static void ExitState(ref PlayerEntity player)
        {
            switch (player.actionState.state)
            {
                case PlayerState.Dodge:
                    player.motion.isRotationLocked = false;
                    break;

                case PlayerState.Swing:
                case PlayerState.Throw:
                    player.motion.isRotationLocked = false;
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
