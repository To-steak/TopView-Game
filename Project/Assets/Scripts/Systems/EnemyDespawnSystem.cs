using System.Collections.Generic;
using TopView.Components;
using TopView.Entities;
using TopView.Requests;

namespace TopView.Systems
{
    // 사망 연출 시간이 끝난 적을 없앤다.
    public static class EnemyDespawnSystem
    {
        /// <summary>
        /// 사망 상태에서 dieTicks가 지난 적의 칸을 그 종류 구간에 반납한다. 게임을 끝내는 적이면 게임오버를 요청한다.
        /// </summary>
        /// <param name="masks">칸별 컴포넌트 표시</param>
        /// <param name="kinds">적 종류 번호</param>
        /// <param name="behaviors">적 행동 값</param>
        /// <param name="actionStates">적 행동 상태</param>
        /// <param name="freeIndicesByKind">적 종류별 구간의 빈 칸 번호 목록</param>
        /// <param name="gameOverRequests">게임오버 요청 큐</param>
        public static void Tick(ComponentMask[] masks, EnemyKindComponent[] kinds, EnemyBehaviorComponent[] behaviors, EnemyActionStateComponent[] actionStates, List<int>[] freeIndicesByKind, Queue<GameOverRequest> gameOverRequests)
        {
            for (int index = 0; index < masks.Length; index++)
            {
                if ((masks[index] & ComponentMask.EnemyArchetype) == ComponentMask.EnemyArchetype
                    && actionStates[index].state == EnemyState.Die
                    && actionStates[index].elapsedTicks >= behaviors[index].dieTicks)
                {
                    SpawnSystem.ReleaseSlot(masks, freeIndicesByKind[kinds[index].kind], index);
                    if (behaviors[index].endsGameOnDeath)
                    {
                        gameOverRequests.Enqueue(new GameOverRequest());
                    }
                }
            }
        }
    }
}
