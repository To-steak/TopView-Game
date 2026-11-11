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
        /// <summary>
        /// 쌓인 상태 변경 요청을 모두 처리한다. 없어졌거나 칸이 재사용된 대상은 건너뛴다. 요청을 넣는 System 바로 뒤에 부른다.
        /// </summary>
        /// <param name="players">플레이어 저장소</param>
        /// <param name="stateChangeRequests">플레이어 상태 변경 요청 큐</param>
        /// <param name="stateRecords">상태 변경 기록</param>
        public static void Tick(PlayerStorage players, Queue<PlayerStateChangeRequest> stateChangeRequests, List<PlayerStateChangedRecord> stateRecords)
        {
            while (stateChangeRequests.Count > 0)
            {
                PlayerStateChangeRequest request = stateChangeRequests.Dequeue();
                ref PlayerEntity entity = ref players.entities[request.target.index];
                if (entity.isAlive && entity.generation == request.target.generation)
                {
                    ChangeState(ref entity.actionState, ref entity.motion, ref entity.rotation, ref entity.mana, in entity.weaponSlots, in entity.movement, in entity.vitals, in entity.upgrade, in entity.upgradeLevels, entity.weaponList.weapons, in request, stateRecords);
                }
            }
        }

        // 지금 상태에서 나가고 새 상태에 들어가 상태 변경을 기록한다. Die 상태에서는 바꾸지 않는다.
        private static void ChangeState(ref PlayerActionStateComponent actionState, ref PlayerMotionComponent motion, ref RotationComponent rotation, ref ManaComponent mana, in PlayerWeaponSlotsComponent weaponSlot, in PlayerMovementComponent movement, in PlayerVitalsComponent vital, in PlayerUpgradeComponent upgrade, in PlayerUpgradeLevelsComponent upgradeLevels, WeaponComponent[] weapons, in PlayerStateChangeRequest request, List<PlayerStateChangedRecord> stateRecords)
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
            actionState.isAttackCommitted = false;

            EnterState(ref actionState, ref motion, ref rotation, ref mana, in weaponSlot, in movement, in vital, in upgrade, in upgradeLevels, weapons, request.moveInput);
        }

        private static void EnterState(ref PlayerActionStateComponent actionState, ref PlayerMotionComponent motion, ref RotationComponent rotation, ref ManaComponent mana, in PlayerWeaponSlotsComponent weaponSlot, in PlayerMovementComponent movement, in PlayerVitalsComponent vital, in PlayerUpgradeComponent upgrade, in PlayerUpgradeLevelsComponent upgradeLevels, WeaponComponent[] weapons, Vector3 moveInput)
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
                    actionState.attackCooldownTicks = PlayerUpgradeUtility.GetAttackIntervalTicks(in weapons[weaponSlot.currentIndex], in upgrade, PlayerUpgradeUtility.GetWeaponLevel(in upgradeLevels, weaponSlot.currentIndex));
                    break;

                case PlayerState.Swing:
                    motion.isRotationLocked = true;
                    actionState.attackCooldownTicks = PlayerUpgradeUtility.GetAttackIntervalTicks(in weapons[weaponSlot.currentIndex], in upgrade, PlayerUpgradeUtility.GetWeaponLevel(in upgradeLevels, weaponSlot.currentIndex));
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
