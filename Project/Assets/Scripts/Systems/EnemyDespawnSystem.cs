using System.Collections.Generic;
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
        /// <param name="enemies">적 저장소</param>
        /// <param name="gameOverRequests">게임오버 요청 큐</param>
        public static void Tick(EnemyStorage enemies, Queue<GameOverRequest> gameOverRequests)
        {
            for (int index = 0; index < enemies.entities.Length; index++)
            {
                ref EnemyEntity entity = ref enemies.entities[index];
                if (entity.isAlive
                    && entity.actionState.state == EnemyState.Die
                    && entity.actionState.elapsedTicks >= entity.behavior.dieTicks)
                {
                    entity.isAlive = false;
                    enemies.freeIndicesByKind[entity.kind.kind].Add(index);
                    if (entity.behavior.endsGameOnDeath)
                    {
                        gameOverRequests.Enqueue(new GameOverRequest());
                    }
                }
            }
        }
    }
}
