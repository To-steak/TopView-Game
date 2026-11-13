using System.Collections.Generic;
using TopView.Components;
using TopView.Entities;
using TopView.Records;
using TopView.Utilities;

namespace TopView.Systems
{
    // 체력이 0이 된 적을 사망 상태로 바꾸고 멈춘다.
    public static class EnemyDeathSystem
    {
        /// <summary>
        /// 체력이 0이고 아직 사망 상태가 아닌 적을 사망 상태로 바꾼다.
        /// </summary>
        /// <param name="enemies">적 저장소</param>
        /// <param name="stateRecords">적 상태 변경 기록</param>
        public static void Tick(EnemyStorage enemies, List<EnemyStateChangedRecord> stateRecords)
        {
            for (int index = 0; index < enemies.entities.Length; index++)
            {
                ref EnemyEntity entity = ref enemies.entities[index];
                if (!entity.isAlive || entity.health.current > 0f || entity.actionState.state == EnemyState.Die)
                {
                    continue;
                }

                stateRecords.Add(new EnemyStateChangedRecord
                {
                    enemy = new EntityIdentifier { index = index, generation = entity.generation },
                    state = EnemyState.Die
                });

                Die(ref entity.actionState, ref entity.agent);
            }
        }

        private static void Die(ref EnemyActionStateComponent actionState, ref NavigationAgentComponent agent)
        {
            actionState.state = EnemyState.Die;
            actionState.elapsedTicks = 0;
            EnemyNavigationUtility.StopMove(ref agent);
        }
    }
}
