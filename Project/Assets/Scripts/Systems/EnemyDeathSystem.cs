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
        // 처리할 적이 가진 컴포넌트
        private const ComponentMask ENEMY_MASK = ComponentMask.Health | ComponentMask.EnemyActionState | ComponentMask.NavigationAgent;

        /// <summary>
        /// 체력이 0이고 아직 사망 상태가 아닌 적을 사망 상태로 바꾼다.
        /// </summary>
        /// <param name="storage">엔티티 저장소. Health, EnemyActionState, NavigationAgent를 가진 칸만 처리하고, 행동 상태와 경로를 고친다</param>
        /// <param name="stateRecords">적 상태 변경 기록</param>
        public static void Tick(EntityStorage storage, List<EnemyStateChangedRecord> stateRecords)
        {
            for (int index = 0; index < storage.entities.Length; index++)
            {
                ref Entity entity = ref storage.entities[index];
                if ((entity.mask & ENEMY_MASK) != ENEMY_MASK || entity.health.current > 0f || entity.enemyActionState.state == EnemyState.Die)
                {
                    continue;
                }

                stateRecords.Add(new EnemyStateChangedRecord
                {
                    enemy = new EntityIdentifier { index = index, generation = entity.generation },
                    state = EnemyState.Die
                });

                Die(ref entity.enemyActionState, ref entity.navigationAgent);
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
