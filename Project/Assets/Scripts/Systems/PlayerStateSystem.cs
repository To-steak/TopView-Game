using System.Collections.Generic;
using TopView.Commands;
using TopView.Components;
using TopView.Entities;
using TopView.Records;
using TopView.Requests;
using UnityEngine;

namespace TopView.Systems
{
    // 플레이어의 행동 상태를 입력과 상태 타이머로 바꾸고, 상태에 들어가고 나갈 때의 값을 정한다.
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
        /// <param name="melees">근접 판정 저장소. 사망할 때 자기 근접 판정을 없앤다</param>
        /// <param name="gameOverRequests">게임오버 요청 큐</param>
        public static void Tick(PlayerStorage players, in PlayerCommand command, List<PlayerStateChangedRecord> stateRecords, Queue<ProjectileSpawnRequest> projectileSpawnRequests, Queue<MeleeSpawnRequest> meleeSpawnRequests, MeleeStorage melees, Queue<GameOverRequest> gameOverRequests)
        {
            for (int playerIndex = 0; playerIndex < players.entities.Length; playerIndex++)
            {
                ref PlayerEntity player = ref players.entities[playerIndex];
                if (!player.isAlive)
                {
                    continue;
                }

                EntityIdentifier identifier = new EntityIdentifier { index = playerIndex, generation = player.generation };
                if (player.health <= 0f && player.state != PlayerState.Die)
                {
                    ChangeState(ref player, in identifier, PlayerState.Die, Vector3.zero, stateRecords);
                    MeleeHitSystem.DespawnOwnedBy(melees, new EntityReference { kind = EntityKind.Player, identifier = identifier });
                }

                player.stateElapsedTicks++;
                HandlePresses(ref player, in identifier, in command, stateRecords);
                UpdateState(ref player, in identifier, in command, stateRecords, projectileSpawnRequests, meleeSpawnRequests, gameOverRequests);
            }
        }

        /// <summary>
        /// 지금 상태에서 나가고 새 상태에 들어간다. Die 상태에서는 바꾸지 않는다.
        /// </summary>
        /// <param name="player">플레이어</param>
        /// <param name="identifier">플레이어를 가리키는 값</param>
        /// <param name="newState">새 상태</param>
        /// <param name="moveInput">이번 틱의 이동 입력(월드). 회피 방향에 쓴다</param>
        /// <param name="stateRecords">상태 변경 기록</param>
        public static void ChangeState(ref PlayerEntity player, in EntityIdentifier identifier, PlayerState newState, Vector3 moveInput, List<PlayerStateChangedRecord> stateRecords)
        {
            if (player.state == PlayerState.Die)
            {
                return;
            }

            ExitState(ref player);

            stateRecords.Add(new PlayerStateChangedRecord
            {
                player = identifier,
                previousState = player.state,
                state = newState
            });

            player.state = newState;
            player.stateElapsedTicks = 0;
            player.isAttackCommitted = false;

            EnterState(ref player, moveInput);
        }

        private static void HandlePresses(ref PlayerEntity player, in EntityIdentifier identifier, in PlayerCommand command, List<PlayerStateChangedRecord> stateRecords)
        {
            if (command.isJumpPressed && IsIdleOrMove(player.state))
            {
                if (player.isGrounded && PlayerStaminaSystem.HasEnoughMana(in player, player.vitals.jumpManaCost))
                {
                    ChangeState(ref player, in identifier, PlayerState.Jump, command.moveDirection, stateRecords);
                }
            }

            if (command.isDodgePressed && IsIdleOrMove(player.state))
            {
                if (player.isGrounded && PlayerStaminaSystem.HasEnoughMana(in player, player.vitals.dodgeManaCost))
                {
                    ChangeState(ref player, in identifier, PlayerState.Dodge, command.moveDirection, stateRecords);
                }
            }

            if (command.isAttackPressed && IsIdleOrMove(player.state) && player.isGrounded)
            {
                if (TryGetAttackState(in player, out PlayerState attackState))
                {
                    ChangeState(ref player, in identifier, attackState, command.moveDirection, stateRecords);
                }
            }

            if (command.swapWeaponIndex >= 0 && IsIdleOrMove(player.state))
            {
                if (CanSwap(in player, command.swapWeaponIndex))
                {
                    player.pendingWeaponIndex = command.swapWeaponIndex;
                    ChangeState(ref player, in identifier, PlayerState.Swap, command.moveDirection, stateRecords);
                }
            }

            if (command.isReloadPressed && IsIdleOrMove(player.state))
            {
                if (CanReload(in player))
                {
                    ChangeState(ref player, in identifier, PlayerState.Reload, command.moveDirection, stateRecords);
                }
            }
        }

        private static void UpdateState(ref PlayerEntity player, in EntityIdentifier identifier, in PlayerCommand command, List<PlayerStateChangedRecord> stateRecords, Queue<ProjectileSpawnRequest> projectileSpawnRequests, Queue<MeleeSpawnRequest> meleeSpawnRequests, Queue<GameOverRequest> gameOverRequests)
        {
            switch (player.state)
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
                    player.moveDirection = command.moveDirection;
                    if (player.verticalVelocity <= 0f && player.isGrounded)
                    {
                        ChangeState(ref player, in identifier, PlayerState.Land, command.moveDirection, stateRecords);
                    }
                    break;

                case PlayerState.Land:
                    player.moveDirection = command.moveDirection;
                    if (player.stateElapsedTicks >= player.action.landTicks)
                    {
                        FinishAction(ref player, in identifier, command.moveDirection, stateRecords);
                    }
                    break;

                case PlayerState.Dodge:
                    if (player.stateElapsedTicks >= player.action.dodgeTicks)
                    {
                        FinishAction(ref player, in identifier, command.moveDirection, stateRecords);
                    }
                    break;

                case PlayerState.Shot:
                case PlayerState.Swing:
                case PlayerState.Throw:
                    UpdateAttack(ref player, in identifier, command.moveDirection, stateRecords, projectileSpawnRequests, meleeSpawnRequests);
                    break;

                case PlayerState.Swap:
                    if (player.stateElapsedTicks >= player.action.swapTicks)
                    {
                        FinishAction(ref player, in identifier, command.moveDirection, stateRecords);
                    }
                    break;

                case PlayerState.Reload:
                    if (player.stateElapsedTicks >= player.action.reloadTicks)
                    {
                        Reload(ref player);
                        FinishAction(ref player, in identifier, command.moveDirection, stateRecords);
                    }
                    break;

                case PlayerState.Die:
                    if (player.stateElapsedTicks == player.action.dieTicks)
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

            player.moveDirection = command.moveDirection;
            player.currentSpeed = isRunning ? player.movement.runSpeed : player.movement.walkSpeed;
            player.isRunning = isRunning;
        }

        private static void UpdateAttack(ref PlayerEntity player, in EntityIdentifier identifier, Vector3 moveInput, List<PlayerStateChangedRecord> stateRecords, Queue<ProjectileSpawnRequest> projectileSpawnRequests, Queue<MeleeSpawnRequest> meleeSpawnRequests)
        {
            ref WeaponComponent weapon = ref player.weapons[player.currentWeaponIndex];

            if (!player.isAttackCommitted && player.stateElapsedTicks >= weapon.attackStartTicks)
            {
                CommitAttack(ref player, in identifier, ref weapon, projectileSpawnRequests, meleeSpawnRequests);
            }

            // Melee는 후딜레이까지 기다리고, Ranged와 Throw는 투사체를 내보내면 바로 끝난다
            bool isFinished = weapon.kind == WeaponKind.Melee
                ? player.stateElapsedTicks >= weapon.attackDurationTicks
                : player.isAttackCommitted;
            if (isFinished)
            {
                FinishAction(ref player, in identifier, moveInput, stateRecords);
            }
        }

        // 공격을 내보낸다. 탄약이 없으면 아무것도 하지 않는다. Ranged와 Throw는 투사체 생성 요청을, Melee는 attackEndTicks까지 이어지는 근접 판정 생성 요청을 넣는다.
        private static void CommitAttack(ref PlayerEntity player, in EntityIdentifier identifier, ref WeaponComponent weapon, Queue<ProjectileSpawnRequest> projectileSpawnRequests, Queue<MeleeSpawnRequest> meleeSpawnRequests)
        {
            player.isAttackCommitted = true;

            switch (weapon.kind)
            {
                case WeaponKind.Ranged:
                    if (weapon.currentAmmo <= 0)
                    {
                        Debug.LogWarning($"PlayerStateSystem >> 탄창 탄약이 {weapon.currentAmmo}이라 원거리 공격을 내보내지 않았다.");
                        return;
                    }
                    weapon.currentAmmo--;
                    RequestProjectile(in player, in weapon, player.currentRotation * Vector3.forward, projectileSpawnRequests);
                    break;

                case WeaponKind.Throw:
                    if (weapon.reserveAmmo <= 0)
                    {
                        Debug.LogWarning($"PlayerStateSystem >> 남은 탄약이 {weapon.reserveAmmo}이라 투척 공격을 내보내지 않았다.");
                        return;
                    }
                    weapon.reserveAmmo--;
                    Vector3 throwDirection = (player.currentRotation * Vector3.forward + Vector3.up * weapon.projectile.upwardRatio).normalized;
                    RequestProjectile(in player, in weapon, throwDirection, projectileSpawnRequests);
                    break;

                case WeaponKind.Melee:
                    meleeSpawnRequests.Enqueue(new MeleeSpawnRequest
                    {
                        owner = new EntityReference { kind = EntityKind.Player, identifier = identifier },
                        faction = Faction.Player,
                        center = player.currentPosition + player.currentRotation * Vector3.forward * weapon.meleeOffset,
                        radius = weapon.meleeRadius,
                        damage = weapon.damage,
                        durationTicks = weapon.attackEndTicks - weapon.attackStartTicks
                    });
                    break;
            }
        }

        // 총구 위치에서 플레이어 회전으로 투사체를 내보낸다.
        private static void RequestProjectile(in PlayerEntity player, in WeaponComponent weapon, Vector3 direction, Queue<ProjectileSpawnRequest> projectileSpawnRequests)
        {
            projectileSpawnRequests.Enqueue(new ProjectileSpawnRequest
            {
                projectile = weapon.projectile,
                faction = Faction.Player,
                damage = weapon.damage,
                position = player.currentPosition + player.currentRotation * player.movement.muzzleOffset,
                rotation = player.currentRotation,
                velocity = direction * weapon.projectile.speed
            });
        }

        // 행동이 끝났을 때 이동 입력에 따라 Idle이나 Move로 돌아간다.
        private static void FinishAction(ref PlayerEntity player, in EntityIdentifier identifier, Vector3 moveInput, List<PlayerStateChangedRecord> stateRecords)
        {
            PlayerState nextState = IsZero(moveInput) ? PlayerState.Idle : PlayerState.Move;
            ChangeState(ref player, in identifier, nextState, moveInput, stateRecords);
        }

        private static void EnterState(ref PlayerEntity player, Vector3 moveInput)
        {
            switch (player.state)
            {
                case PlayerState.Idle:
                    player.moveDirection = Vector3.zero;
                    break;

                case PlayerState.Jump:
                    player.verticalVelocity = player.movement.jumpPower;
                    PlayerStaminaSystem.UseMana(ref player, player.vitals.jumpManaCost);
                    player.currentSpeed = player.movement.inAirSpeed;
                    break;

                case PlayerState.Dodge:
                    player.isRotationLocked = true;
                    // 이동 입력이 있으면 그쪽으로, 서 있으면 바라보는 쪽(XZ)으로 회피한다
                    Vector3 dodgeDirection = moveInput;
                    if (IsZero(dodgeDirection))
                    {
                        dodgeDirection = player.currentRotation * Vector3.forward;
                        dodgeDirection.y = 0f;
                        dodgeDirection.Normalize();
                    }
                    player.moveDirection = dodgeDirection;
                    player.currentSpeed = player.movement.dodgeSpeed;
                    if (IsZero(player.moveDirection))
                    {
                        Debug.LogWarning("PlayerStateSystem >> 회피 방향을 정하지 못해 회피 방향으로 돌지 않았다.");
                    }
                    else
                    {
                        player.currentRotation = Quaternion.LookRotation(player.moveDirection);
                    }
                    PlayerStaminaSystem.UseMana(ref player, player.vitals.dodgeManaCost);
                    break;

                case PlayerState.Shot:
                case PlayerState.Swing:
                case PlayerState.Throw:
                    player.isRotationLocked = true;
                    player.moveDirection = Vector3.zero;
                    break;

                case PlayerState.Swap:
                    player.moveDirection = Vector3.zero;
                    player.currentWeaponIndex = player.pendingWeaponIndex;
                    break;

                case PlayerState.Reload:
                    player.moveDirection = Vector3.zero;
                    break;

                case PlayerState.Die:
                    player.moveDirection = Vector3.zero;
                    player.isRotationLocked = true;
                    break;
            }
        }

        private static void ExitState(ref PlayerEntity player)
        {
            switch (player.state)
            {
                case PlayerState.Dodge:
                    player.isRotationLocked = false;
                    break;

                case PlayerState.Shot:
                case PlayerState.Swing:
                case PlayerState.Throw:
                    player.isRotationLocked = false;
                    break;
            }
        }

        // 들고 있는 무기로 들어갈 공격 상태. 탄약이 없거나 무기가 없으면 false
        private static bool TryGetAttackState(in PlayerEntity player, out PlayerState attackState)
        {
            attackState = PlayerState.Idle;
            if (!IsValidWeaponIndex(in player, player.currentWeaponIndex))
            {
                return false;
            }

            ref readonly WeaponComponent weapon = ref player.weapons[player.currentWeaponIndex];
            switch (weapon.kind)
            {
                case WeaponKind.Melee:
                    attackState = PlayerState.Swing;
                    return true;

                case WeaponKind.Ranged:
                    attackState = PlayerState.Shot;
                    return weapon.currentAmmo > 0;

                case WeaponKind.Throw:
                    attackState = PlayerState.Throw;
                    return weapon.reserveAmmo > 0;
            }

            return false;
        }

        private static bool CanSwap(in PlayerEntity player, int weaponIndex)
        {
            return weaponIndex != player.currentWeaponIndex && IsValidWeaponIndex(in player, weaponIndex);
        }

        private static bool CanReload(in PlayerEntity player)
        {
            if (!IsValidWeaponIndex(in player, player.currentWeaponIndex))
            {
                return false;
            }

            ref readonly WeaponComponent weapon = ref player.weapons[player.currentWeaponIndex];
            return weapon.kind == WeaponKind.Ranged && weapon.currentAmmo < weapon.maxAmmo && weapon.reserveAmmo > 0;
        }

        // 예비 탄약에서 모자란 만큼 장전한다.
        private static void Reload(ref PlayerEntity player)
        {
            ref WeaponComponent weapon = ref player.weapons[player.currentWeaponIndex];
            int loadAmmo = Mathf.Min(weapon.maxAmmo - weapon.currentAmmo, weapon.reserveAmmo);
            weapon.currentAmmo += loadAmmo;
            weapon.reserveAmmo -= loadAmmo;
        }

        private static bool IsValidWeaponIndex(in PlayerEntity player, int weaponIndex)
        {
            return weaponIndex >= 0 && weaponIndex < player.weapons.Length;
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
