using System.Collections.Generic;
using TopView.Components;
using TopView.Entities;
using TopView.Requests;

namespace TopView.Systems
{
    // 주인이 사라진 무기의 소멸을 요청한다. 주인이 사망 상태에 들어간 것만으로는 요청하지 않는다.
    public static class WeaponOwnerDeathSystem
    {
        /// <summary>
        /// 주인의 칸이 비었거나 다른 엔티티로 바뀐 무기마다 소멸 요청을 넣는다. WeaponDespawnSystem 바로 앞에 부른다.
        /// </summary>
        /// <param name="weapons">무기 저장소</param>
        /// <param name="players">플레이어 저장소</param>
        /// <param name="despawnRequests">무기 소멸 요청 큐</param>
        public static void Tick(WeaponStorage weapons, PlayerStorage players, Queue<WeaponDespawnRequest> despawnRequests)
        {
            for (int index = 0; index < weapons.entities.Length; index++)
            {
                ref readonly WeaponEntity entity = ref weapons.entities[index];
                if (!entity.isAlive)
                {
                    continue;
                }

                if (!HasOwner(in entity.owner, players.entities))
                {
                    despawnRequests.Enqueue(new WeaponDespawnRequest { target = new EntityIdentifier { index = index, generation = entity.generation } });
                }
            }
        }

        // 주인의 칸에 그 주인이 아직 살아 있는지
        private static bool HasOwner(in OwnerComponent owner, PlayerEntity[] players)
        {
            ref readonly PlayerEntity player = ref players[owner.owner.index];
            return player.isAlive && player.generation == owner.owner.generation;
        }
    }
}
