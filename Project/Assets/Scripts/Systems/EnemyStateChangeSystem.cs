using System.Collections.Generic;
using TopView.Components;
using TopView.Entities;
using TopView.Records;
using TopView.Requests;
using TopView.Utilities;

namespace TopView.Systems
{
    // 상태 변경 요청을 받은 적의 행동 상태를 바꾸고, 상태에 들어갈 때의 값을 정한다.
    public static class EnemyStateChangeSystem
    {
        /// <summary>
        /// 쌓인 상태 변경 요청을 모두 처리한다. 없어졌거나 칸이 재사용된 대상은 건너뛴다. 요청을 넣는 System 바로 뒤에 부른다.
        /// </summary>
        /// <param name="enemies">적 저장소</param>
        /// <param name="stateChangeRequests">적 상태 변경 요청 큐</param>
        /// <param name="stateRecords">적 상태 변경 기록</param>
        public static void Tick(EnemyStorage enemies, Queue<EnemyStateChangeRequest> stateChangeRequests, List<EnemyStateChangedRecord> stateRecords)
        {
            while (stateChangeRequests.Count > 0)
            {
                EnemyStateChangeRequest request = stateChangeRequests.Dequeue();
                ref EnemyEntity entity = ref enemies.entities[request.target.index];
                if (entity.isAlive && entity.generation == request.target.generation)
                {
                    ChangeState(ref entity.actionState, ref entity.agent, entity.attackList.attacks, in request, stateRecords);
                }
            }
        }

        // 새 상태에 들어가 상태 변경을 기록한다. Die 상태에서는 바꾸지 않는다.
        private static void ChangeState(ref EnemyActionStateComponent actionState, ref NavigationAgentComponent agent, EnemyAttackComponent[] attacks, in EnemyStateChangeRequest request, List<EnemyStateChangedRecord> stateRecords)
        {
            if (actionState.state == EnemyState.Die)
            {
                return;
            }

            stateRecords.Add(new EnemyStateChangedRecord
            {
                enemy = request.target,
                state = request.state,
                attackIndex = request.state == EnemyState.Attack ? actionState.attackIndex : default
            });

            actionState.state = request.state;
            actionState.elapsedTicks = 0;

            switch (request.state)
            {
                case EnemyState.Idle:
                    EnemyNavigationUtility.StopMove(ref agent);
                    break;
                case EnemyState.Attack:
                    EnemyNavigationUtility.StopMove(ref agent);
                    break;
            }
        }
    }
}
