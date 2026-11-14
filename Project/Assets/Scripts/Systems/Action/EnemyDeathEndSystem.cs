using System.Collections.Generic;
using TopView.Components;
using TopView.Entities;
using TopView.Requests;
using TopView.Utilities;

namespace TopView.Systems
{
    // 사망 연출 시간이 끝난 적의 소멸을 요청하고, 드롭 표에서 전리품을 뽑는다. 칸을 비우는 일은 EnemyDespawnSystem이 한다.
    public static class EnemyDeathEndSystem
    {
        // 처리할 적이 가진 컴포넌트
        private const ComponentMask ENEMY_MASK = ComponentMask.Position | ComponentMask.EnemyKind | ComponentMask.EnemyActionState | ComponentMask.Death | ComponentMask.LootList | ComponentMask.Combat;

        /// <summary>
        /// 사망 상태에서 dieTicks가 지난 적의 소멸을 요청하고, 드롭 표에서 뽑은 전리품의 생성을 요청한다. 게임을 끝내는 적이면 승리로 끝내 달라고 요청한다. 바로 뒤에 EnemyDespawnSystem을 부른다.
        /// </summary>
        /// <param name="storage">엔티티 저장소. 적(Position, EnemyKind, EnemyActionState, Death, LootList, Combat)을 읽기만 한다</param>
        /// <param name="gameOverRequests">게임오버 요청 큐</param>
        /// <param name="random">시드 고정 난수 생성기</param>
        /// <param name="lootSpawnRequests">전리품 생성 요청 큐</param>
        /// <param name="despawnRequests">적 소멸 요청 큐</param>
        public static void Tick(EntityStorage storage, Queue<GameOverRequest> gameOverRequests, System.Random random, Queue<LootSpawnRequest> lootSpawnRequests, Queue<EnemyDespawnRequest> despawnRequests)
        {
            for (int index = 0; index < storage.entities.Length; index++)
            {
                ref readonly Entity entity = ref storage.entities[index];
                if ((entity.mask & ENEMY_MASK) != ENEMY_MASK
                    || entity.enemyActionState.state != EnemyState.Die
                    || entity.enemyActionState.elapsedTicks < entity.death.dieTicks)
                {
                    continue;
                }

                despawnRequests.Enqueue(new EnemyDespawnRequest { target = new EntityIdentifier { index = index, generation = entity.generation } });
                if (LootUtility.TryRoll(entity.lootList.loots, entity.combat.level, random, out LootKind lootKind, out int lootAmount))
                {
                    lootSpawnRequests.Enqueue(new LootSpawnRequest
                    {
                        kind = lootKind,
                        amount = lootAmount,
                        position = entity.position.current
                    });
                }
                if (entity.death.isGameOverOnDeath)
                {
                    gameOverRequests.Enqueue(new GameOverRequest { isVictory = true });
                }
            }
        }
    }
}
