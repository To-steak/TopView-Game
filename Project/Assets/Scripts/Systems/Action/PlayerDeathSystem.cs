using System.Collections.Generic;
using TopView.Components;
using TopView.Entities;
using TopView.Records;
using UnityEngine;

namespace TopView.Systems
{
    // 체력이 0이 된 플레이어를 사망 상태로 바꾼다.
    public static class PlayerDeathSystem
    {
        // 처리할 플레이어가 가진 컴포넌트
        private const ComponentMask PLAYER_MASK = ComponentMask.PlayerActionState | ComponentMask.Health | ComponentMask.PlayerMotion;

        /// <summary>
        /// 체력이 0이고 아직 사망 상태가 아닌 플레이어를 사망 상태로 바꾼다. 움직임을 멈추고 회전을 잠근다.
        /// </summary>
        /// <param name="storage">엔티티 저장소. 플레이어(PlayerActionState, Health, PlayerMotion)만 처리한다</param>
        /// <param name="stateRecords">상태 변경 기록</param>
        public static void Tick(EntityStorage storage, List<PlayerStateChangedRecord> stateRecords)
        {
            for (int index = 0; index < storage.entities.Length; index++)
            {
                ref Entity entity = ref storage.entities[index];
                if ((entity.mask & PLAYER_MASK) != PLAYER_MASK || entity.health.current > 0f || entity.playerActionState.state == PlayerState.Die)
                {
                    continue;
                }

                EntityIdentifier identifier = new EntityIdentifier { index = index, generation = entity.generation };
                stateRecords.Add(new PlayerStateChangedRecord
                {
                    player = identifier,
                    previousState = entity.playerActionState.state,
                    state = PlayerState.Die
                });

                Die(ref entity.playerActionState, ref entity.playerMotion);
            }
        }

        private static void Die(ref PlayerActionStateComponent actionState, ref PlayerMotionComponent motion)
        {
            actionState.state = PlayerState.Die;
            actionState.elapsedTicks = 0;
            actionState.committedShotCount = 0;

            motion.moveDirection = Vector3.zero;
            motion.isRotationLocked = true;
        }
    }
}
