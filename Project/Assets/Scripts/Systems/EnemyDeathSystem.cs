using System.Collections.Generic;
using TopView.Components;
using TopView.Entities;
using TopView.Records;

namespace TopView.Systems
{
    // 체력이 0이 된 적을 사망 상태로 바꾸고 멈춘다.
    public static class EnemyDeathSystem
    {
        /// <summary>
        /// 체력이 0이고 아직 사망 상태가 아닌 적을 사망 상태로 바꾼다.
        /// </summary>
        /// <param name="masks">칸별 컴포넌트 표시</param>
        /// <param name="generations">칸이 채워진 횟수</param>
        /// <param name="healths">체력</param>
        /// <param name="actionStates">적 행동 상태와 공격 진행</param>
        /// <param name="agents">목적지와 경로</param>
        /// <param name="stateRecords">적 상태 변경 기록</param>
        public static void Tick(ComponentMask[] masks, int[] generations, HealthComponent[] healths, EnemyActionStateComponent[] actionStates, NavigationAgentComponent[] agents, List<EnemyStateChangedRecord> stateRecords)
        {
            for (int index = 0; index < masks.Length; index++)
            {
                if ((masks[index] & ComponentMask.EnemyArchetype) != ComponentMask.EnemyArchetype)
                {
                    continue;
                }

                ref EnemyActionStateComponent actionState = ref actionStates[index];
                if (healths[index].current > 0f || actionState.state == EnemyState.Die)
                {
                    continue;
                }

                stateRecords.Add(new EnemyStateChangedRecord
                {
                    enemy = new EntityIdentifier { index = index, generation = generations[index] },
                    previousState = actionState.state,
                    state = EnemyState.Die
                });

                actionState.state = EnemyState.Die;
                actionState.elapsedTicks = 0;
                EnemyNavigationSystem.StopMove(ref agents[index]);
            }
        }
    }
}
