using System.Collections.Generic;
using TopView.Components;
using TopView.Entities;
using TopView.Requests;

namespace TopView.Systems
{
    // 공격한 플레이어가 사망한 근접 판정의 소멸을 요청한다.
    public static class MeleeOwnerDeathSystem
    {
        /// <summary>
        /// 플레이어 진영의 근접 판정 중 주인이 사망 상태인 판정을 찾아 소멸 요청을 넣는다. PlayerDeathSystem 다음에 부른다.
        /// 저장소마다 칸 번호를 따로 쓰므로 진영으로 플레이어의 공격만 고른 뒤 주인을 찾는다.
        /// </summary>
        /// <param name="melees">근접 판정 저장소</param>
        /// <param name="players">플레이어 저장소</param>
        /// <param name="despawnRequests">근접 판정 소멸 요청 큐</param>
        public static void Tick(MeleeStorage melees, PlayerStorage players, Queue<MeleeDespawnRequest> despawnRequests)
        {
            for (int index = 0; index < melees.entities.Length; index++)
            {
                ref readonly MeleeEntity entity = ref melees.entities[index];
                if (!entity.isAlive || entity.payload.faction != Faction.Player)
                {
                    continue;
                }

                ref readonly EntityIdentifier owner = ref entity.melee.owner;
                ref readonly PlayerEntity player = ref players.entities[owner.index];
                if (player.isAlive && player.generation == owner.generation && player.actionState.state == PlayerState.Die)
                {
                    despawnRequests.Enqueue(new MeleeDespawnRequest { target = new EntityIdentifier { index = index, generation = entity.generation } });
                }
            }
        }
    }
}
