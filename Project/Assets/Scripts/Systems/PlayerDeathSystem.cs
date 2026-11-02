using System.Collections.Generic;
using TopView.Components;
using TopView.Entities;
using TopView.Records;
using UnityEngine;

namespace TopView.Systems
{
    // 체력이 0이 된 플레이어를 사망 상태로 바꾸고 자기 근접 판정을 없앤다.
    public static class PlayerDeathSystem
    {
        /// <summary>
        /// 체력이 0이고 아직 사망 상태가 아닌 플레이어를 사망 상태로 바꾼다. 움직임을 멈추고 회전을 잠근다.
        /// </summary>
        /// <param name="masks">칸별 컴포넌트 표시</param>
        /// <param name="generations">칸이 채워진 횟수</param>
        /// <param name="healths">체력</param>
        /// <param name="actionStates">플레이어 행동 상태와 공격 진행</param>
        /// <param name="motions">플레이어 이동 상태</param>
        /// <param name="melees">근접 판정 값과 진행 시간</param>
        /// <param name="freeMeleeIndices">근접 판정 구간의 빈 칸 번호 목록</param>
        /// <param name="stateRecords">상태 변경 기록</param>
        public static void Tick(ComponentMask[] masks, int[] generations, HealthComponent[] healths, PlayerActionStateComponent[] actionStates, PlayerMotionComponent[] motions, MeleeComponent[] melees, List<int> freeMeleeIndices, List<PlayerStateChangedRecord> stateRecords)
        {
            for (int index = 0; index < masks.Length; index++)
            {
                if ((masks[index] & ComponentMask.PlayerArchetype) != ComponentMask.PlayerArchetype)
                {
                    continue;
                }

                ref PlayerActionStateComponent actionState = ref actionStates[index];
                if (healths[index].current > 0f || actionState.state == PlayerState.Die)
                {
                    continue;
                }

                EntityIdentifier identifier = new EntityIdentifier { index = index, generation = generations[index] };
                stateRecords.Add(new PlayerStateChangedRecord
                {
                    player = identifier,
                    previousState = actionState.state,
                    state = PlayerState.Die
                });

                actionState.state = PlayerState.Die;
                actionState.elapsedTicks = 0;
                actionState.isAttackCommitted = false;

                motions[index].moveDirection = Vector3.zero;
                motions[index].isRotationLocked = true;

                MeleeHitSystem.DespawnOwnedBy(masks, melees, freeMeleeIndices, in identifier);
            }
        }
    }
}
