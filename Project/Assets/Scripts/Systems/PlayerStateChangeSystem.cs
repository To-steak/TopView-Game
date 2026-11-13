using System.Collections.Generic;
using TopView.Components;
using TopView.Entities;
using TopView.Records;
using TopView.Requests;
using TopView.Utilities;
using UnityEngine;

namespace TopView.Systems
{
    // 상태 변경 요청을 받은 플레이어의 행동 상태를 바꾸고, 상태에 들어가고 나갈 때의 값을 정한다.
    public static class PlayerStateChangeSystem
    {
        // 공격 간격을 정하는 무기가 가진 컴포넌트
        private const ComponentMask ATTACK_INTERVAL_MASK = ComponentMask.AttackInterval | ComponentMask.WeaponUpgrade | ComponentMask.UpgradeLevel;
        // 처리할 플레이어가 가진 컴포넌트
        private const ComponentMask PLAYER_MASK = ComponentMask.PlayerActionState | ComponentMask.Rotation | ComponentMask.PlayerMovement | ComponentMask.PlayerVitals | ComponentMask.PlayerUpgrade | ComponentMask.PlayerMotion | ComponentMask.Mana | ComponentMask.WeaponSlots | ComponentMask.PlayerUpgradeLevels;

        /// <summary>
        /// 쌓인 상태 변경 요청을 모두 처리한다. 없어졌거나 칸이 재사용된 대상은 건너뛴다. 요청을 넣는 System 바로 뒤에 부른다.
        /// </summary>
        /// <param name="storage">엔티티 저장소. 플레이어(PlayerActionState, Rotation, PlayerMovement, PlayerVitals, PlayerUpgrade, PlayerMotion, Mana, WeaponSlots, PlayerUpgradeLevels)만 처리한다</param>
        /// <param name="stateChangeRequests">플레이어 상태 변경 요청 큐</param>
        /// <param name="stateRecords">상태 변경 기록</param>
        public static void Tick(EntityStorage storage, Queue<PlayerStateChangeRequest> stateChangeRequests, List<PlayerStateChangedRecord> stateRecords)
        {
            while (stateChangeRequests.Count > 0)
            {
                PlayerStateChangeRequest request = stateChangeRequests.Dequeue();
                ref Entity entity = ref storage.entities[request.target.index];
                if ((entity.mask & PLAYER_MASK) == PLAYER_MASK && entity.generation == request.target.generation)
                {
                    ref readonly Entity weapon = ref storage.entities[entity.weaponSlots.weapons[entity.weaponSlots.currentIndex].index];
                    int attackIntervalTicks = GetAttackIntervalTicks(in weapon, in entity.playerUpgrade);
                    ChangeState(ref entity.playerActionState, ref entity.playerMotion, ref entity.rotation, ref entity.mana, in entity.playerMovement, in entity.playerVitals, in entity.playerUpgrade, in entity.playerUpgradeLevels, attackIntervalTicks, in request, stateRecords);
                }
            }
        }

        // 강화가 반영된 무기의 공격 간격(틱). AttackInterval이 없는 무기는 0이라 바로 다시 공격할 수 있다.
        private static int GetAttackIntervalTicks(in Entity weapon, in PlayerUpgradeComponent upgrade)
        {
            if ((weapon.mask & ATTACK_INTERVAL_MASK) != ATTACK_INTERVAL_MASK)
            {
                return 0;
            }

            return PlayerUpgradeUtility.GetAttackIntervalTicks(in weapon.attackInterval, in weapon.weaponUpgrade, in upgrade, weapon.upgradeLevel.level);
        }

        // 지금 상태에서 나가고 새 상태에 들어가 상태 변경을 기록한다. Die 상태에서는 바꾸지 않는다.
        private static void ChangeState(ref PlayerActionStateComponent actionState, ref PlayerMotionComponent motion, ref RotationComponent rotation, ref ManaComponent mana, in PlayerMovementComponent movement, in PlayerVitalsComponent vital, in PlayerUpgradeComponent upgrade, in PlayerUpgradeLevelsComponent upgradeLevels, int attackIntervalTicks, in PlayerStateChangeRequest request, List<PlayerStateChangedRecord> stateRecords)
        {
            if (actionState.state == PlayerState.Die)
            {
                return;
            }

            ExitState(in actionState, ref motion);

            stateRecords.Add(new PlayerStateChangedRecord
            {
                player = request.target,
                previousState = actionState.state,
                state = request.state
            });

            actionState.state = request.state;
            actionState.elapsedTicks = 0;
            actionState.committedShotCount = 0;

            EnterState(ref actionState, ref motion, ref rotation, ref mana, in movement, in vital, in upgrade, in upgradeLevels, attackIntervalTicks, request.moveInput);
        }

        private static void EnterState(ref PlayerActionStateComponent actionState, ref PlayerMotionComponent motion, ref RotationComponent rotation, ref ManaComponent mana, in PlayerMovementComponent movement, in PlayerVitalsComponent vital, in PlayerUpgradeComponent upgrade, in PlayerUpgradeLevelsComponent upgradeLevels, int attackIntervalTicks, Vector3 moveInput)
        {
            switch (actionState.state)
            {
                case PlayerState.Idle:
                    motion.moveDirection = Vector3.zero;
                    break;

                case PlayerState.Jump:
                    motion.verticalVelocity = movement.jumpPower;
                    PlayerStaminaUtility.UseMana(ref mana, PlayerUpgradeUtility.GetJumpManaCost(in vital, in upgrade, in upgradeLevels));
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
                        Debug.LogWarning("PlayerStateChangeSystem >> 회피 방향을 정하지 못해 회피 방향으로 돌지 않았다.");
                    }
                    else
                    {
                        rotation.current = Quaternion.LookRotation(motion.moveDirection);
                    }
                    PlayerStaminaUtility.UseMana(ref mana, PlayerUpgradeUtility.GetDodgeManaCost(in vital, in upgrade, in upgradeLevels));
                    break;

                case PlayerState.Shot:
                    actionState.attackCooldownTicks = attackIntervalTicks;
                    break;

                case PlayerState.Swing:
                    motion.isRotationLocked = true;
                    actionState.attackCooldownTicks = attackIntervalTicks;
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
