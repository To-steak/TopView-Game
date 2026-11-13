using System.Collections.Generic;
using TopView.Entities;
using TopView.Requests;

namespace TopView.Systems
{
    // 공격한 플레이어가 사망한 근접 판정의 소멸을 요청한다.
    public static class MeleeOwnerDeathSystem
    {
        /// <summary>
        /// 주인이 사망 상태인 플레이어인 근접 판정을 찾아 소멸 요청을 넣는다. PlayerDeathSystem 다음에 부른다.
        /// </summary>
        /// <param name="storage">엔티티 저장소. Melee를 가진 칸의 주인이 PlayerActionState를 가졌는지 본다</param>
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
                ref readonly Entity player = ref storage.entities[owner.index];
                if ((player.mask & ComponentMask.PlayerActionState) != 0 && player.generation == owner.generation && player.playerActionState.state == PlayerState.Die)
                {
                    despawnRequests.Enqueue(new MeleeDespawnRequest { target = new EntityIdentifier { index = index, generation = entity.generation } });
                }
            }
        }
    }
}
