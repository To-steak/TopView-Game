using System.Collections.Generic;
using TopView.Entities;
using TopView.Requests;

namespace TopView.Systems
{
    // 공격한 플레이어나 적이 사망한 근접 판정의 소멸을 요청한다.
    public static class MeleeOwnerDeathSystem
    {
        /// <summary>
        /// 주인이 사망 상태인 플레이어나 적인 근접 판정을 찾아 소멸 요청을 넣는다. PlayerDeathSystem과 EnemyDeathSystem 다음에 부른다.
        /// </summary>
        /// <param name="storage">엔티티 저장소. Melee를 가진 칸의 주인이 PlayerActionState나 EnemyActionState를 가졌으면 그 행동 상태를 본다</param>
        /// <param name="despawnRequests">근접 판정 소멸 요청 큐</param>
        public static void Tick(EntityStorage storage, Queue<MeleeDespawnRequest> despawnRequests)
        {
            for (int index = 0; index < storage.entities.Length; index++)
            {
                ref readonly Entity entity = ref storage.entities[index];
                if ((entity.mask & ComponentMask.Melee) == 0)
                {
                    continue;
                }

                ref readonly EntityIdentifier owner = ref entity.melee.owner;
                ref readonly Entity ownerEntity = ref storage.entities[owner.index];
                if (ownerEntity.generation == owner.generation && IsDead(in ownerEntity))
                {
                    despawnRequests.Enqueue(new MeleeDespawnRequest { target = new EntityIdentifier { index = index, generation = entity.generation } });
                }
            }
        }

        // 플레이어나 적이 사망 상태인지 본다. 행동 상태가 없는 칸은 사망하지 않은 것으로 본다.
        private static bool IsDead(in Entity entity)
        {
            if ((entity.mask & ComponentMask.PlayerActionState) != 0)
            {
                return entity.playerActionState.state == PlayerState.Die;
            }

            if ((entity.mask & ComponentMask.EnemyActionState) != 0)
            {
                return entity.enemyActionState.state == EnemyState.Die;
            }

            return false;
        }
    }
}
