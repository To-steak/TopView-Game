using System.Collections.Generic;
using TopView.Components;
using TopView.Entities;
using TopView.Requests;
using TopView.Utilities;

namespace TopView.Systems
{
    // 사망 연출 시간이 끝난 적을 없애고, 드롭 표에서 전리품을 뽑는다.
    public static class EnemyDespawnSystem
    {
        /// <summary>
        /// 사망 상태에서 dieTicks가 지난 적의 칸을 그 종류 구간에 반납하고, 드롭 표에서 뽑은 전리품의 생성을 요청한다. 게임을 끝내는 적이면 승리로 끝내 달라고 요청한다.
        /// </summary>
        /// <param name="enemies">적 저장소</param>
        /// <param name="gameOverRequests">게임오버 요청 큐</param>
        /// <param name="random">시드 고정 난수 생성기</param>
        /// <param name="lootSpawnRequests">전리품 생성 요청 큐</param>
        public static void Tick(EnemyStorage enemies, Queue<GameOverRequest> gameOverRequests, System.Random random, Queue<LootSpawnRequest> lootSpawnRequests)
        {
            for (int index = 0; index < enemies.entities.Length; index++)
            {
                ref EnemyEntity entity = ref enemies.entities[index];
                if (entity.isAlive
                    && entity.actionState.state == EnemyState.Die
                    && entity.actionState.elapsedTicks >= entity.death.dieTicks)
                {
                    entity.isAlive = false;
                    enemies.freeIndicesByKind[entity.kind.kind].Add(index);
                    if (LootUtility.TryRoll(entity.lootList.loots, entity.combat.level, random, out LootKind lootKind, out int lootAmount))
                    {
                        lootSpawnRequests.Enqueue(new LootSpawnRequest
                        {
                            kind = lootKind,
                            amount = lootAmount,
                            position = entity.position.current
                        });
                    }
                    if (entity.death.endsGameOnDeath)
                    {
                        gameOverRequests.Enqueue(new GameOverRequest { isVictory = true });
                    }
                }
            }
        }
    }
}
