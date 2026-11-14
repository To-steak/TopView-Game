using System.Collections.Generic;
using NUnit.Framework;
using TopView.Components;
using TopView.Entities;
using TopView.Records;
using TopView.Requests;
using TopView.Systems;
using UnityEngine;

namespace TopView.Tests.EditMode
{
    // 회피가 끝날 때 회피 방향의 이동이 남지 않는지 확인한다.
    // 회피가 끝나는 틱에는 PlayerMoveInputSystem이 이미 지나가서, 여기서 남긴 이동 방향과 속도로 PlayerMovementSystem이 그 틱을 움직인다.
    public class PlayerStateChangeSystemTests
    {
        private EntityStorage _storage;
        private Queue<PlayerStateChangeRequest> _stateChangeRequests;
        private List<PlayerStateChangedRecord> _stateRecords;
        private static readonly Vector3 DODGE_DIRECTION = Vector3.forward;
        private static readonly Vector3 MOVE_INPUT = Vector3.right;

        // 저장소의 칸 번호. 구간 순서(플레이어, 무기)대로 칸을 하나씩 둔다
        private const int PLAYER_INDEX = 0;
        private const int WEAPON_INDEX = 1;
        private const float DODGE_SPEED = 12f;

        /// <summary>
        /// 플레이어와 무기 구간에 칸이 하나씩 있는 저장소에 회피 중인 플레이어를 놓는다.
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            _storage = new EntityStorage(0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 0, 0);
            _stateChangeRequests = new Queue<PlayerStateChangeRequest>();
            _stateRecords = new List<PlayerStateChangedRecord>();

            ref Entity player = ref _storage.entities[PLAYER_INDEX];
            player.mask = ComponentMask.PlayerActionState | ComponentMask.Rotation | ComponentMask.PlayerMovement | ComponentMask.PlayerVitals | ComponentMask.PlayerUpgrade
                | ComponentMask.PlayerMotion | ComponentMask.Mana | ComponentMask.WeaponSlots | ComponentMask.PlayerUpgradeLevels;
            player.generation = 1;
            player.playerActionState.state = PlayerState.Dodge;
            player.rotation = new RotationComponent { previous = Quaternion.identity, current = Quaternion.identity };
            player.playerMovement = new PlayerMovementComponent { walkSpeed = 4f, dodgeSpeed = DODGE_SPEED };
            player.playerMotion = new PlayerMotionComponent { moveDirection = DODGE_DIRECTION, currentSpeed = DODGE_SPEED, isRotationLocked = true };
            player.weaponSlots.weapons = new EntityIdentifier[] { new EntityIdentifier { index = WEAPON_INDEX, generation = 1 } };
        }

        /// <summary>
        /// 회피가 Idle로 끝나면 회피 방향의 이동을 멈춘다.
        /// </summary>
        [Test]
        public void StopsDodgeMovementWhenEndingInIdle()
        {
            RequestState(PlayerState.Idle, Vector3.zero);

            PlayerStateChangeSystem.Tick(_storage, _stateChangeRequests, _stateRecords);

            Assert.AreEqual(Vector3.zero, _storage.entities[PLAYER_INDEX].playerMotion.moveDirection);
        }

        /// <summary>
        /// 회피가 Move로 끝나도 회피 방향의 이동을 멈춘다. 남기면 끝나는 틱에 회피 방향으로 회피 속도만큼 한 틱 더 간다.
        /// 이동 입력은 다음 틱의 PlayerMoveInputSystem이 반영한다.
        /// </summary>
        [Test]
        public void StopsDodgeMovementWhenEndingInMove()
        {
            RequestState(PlayerState.Move, MOVE_INPUT);

            PlayerStateChangeSystem.Tick(_storage, _stateChangeRequests, _stateRecords);

            Assert.AreNotEqual(DODGE_DIRECTION, _storage.entities[PLAYER_INDEX].playerMotion.moveDirection, "회피가 Move로 끝났는데 회피 방향의 이동이 남았다.");
        }

        private void RequestState(PlayerState state, Vector3 moveInput)
        {
            _stateChangeRequests.Enqueue(new PlayerStateChangeRequest
            {
                target = new EntityIdentifier { index = PLAYER_INDEX, generation = 1 },
                state = state,
                moveInput = moveInput
            });
        }
    }
}
