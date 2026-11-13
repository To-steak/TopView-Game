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
        /// <summary>
        /// 체력이 0이고 아직 사망 상태가 아닌 플레이어를 사망 상태로 바꾼다. 움직임을 멈추고 회전을 잠근다.
        /// </summary>
        /// <param name="players">플레이어 저장소</param>
        /// <param name="stateRecords">상태 변경 기록</param>
        public static void Tick(PlayerStorage players, List<PlayerStateChangedRecord> stateRecords)
        {
            for (int index = 0; index < players.entities.Length; index++)
            {
                ref PlayerEntity entity = ref players.entities[index];
                if (!entity.isAlive || entity.health.current > 0f || entity.actionState.state == PlayerState.Die)
                {
                    continue;
                }

                EntityIdentifier identifier = new EntityIdentifier { index = index, generation = entity.generation };
                stateRecords.Add(new PlayerStateChangedRecord
                {
                    player = identifier,
                    previousState = entity.actionState.state,
                    state = PlayerState.Die
                });

                Die(ref entity.actionState, ref entity.motion);
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
