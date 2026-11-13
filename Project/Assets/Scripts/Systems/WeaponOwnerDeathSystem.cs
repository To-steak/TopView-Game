using System.Collections.Generic;
using TopView.Components;
using TopView.Entities;
using TopView.Requests;

namespace TopView.Systems
{
    // 주인이 사라진 무기의 소멸을 요청한다. 주인이 사망 상태에 들어간 것만으로는 요청하지 않는다.
    public static class WeaponOwnerDeathSystem
    {
        // 처리할 플레이어가 가진 컴포넌트
        private const ComponentMask PLAYER_MASK = ComponentMask.PlayerActionState;

        /// <summary>
        /// 주인의 칸이 비었거나 다른 엔티티로 바뀐 무기마다 소멸 요청을 넣는다. WeaponDespawnSystem 바로 앞에 부른다.
        /// </summary>
        /// <param name="weapons">무기 저장소</param>
        /// <param name="storage">엔티티 저장소. 플레이어(PlayerActionState)만 처리한다</param>
        /// <param name="despawnRequests">무기 소멸 요청 큐</param>
        public static void Tick(WeaponStorage weapons, EntityStorage storage, Queue<WeaponDespawnRequest> despawnRequests)
        {
            for (int index = 0; index < weapons.entities.Length; index++)
            {
                ref readonly WeaponEntity entity = ref weapons.entities[index];
                if (!entity.isAlive)
                {
                    continue;
                }

                if (!HasOwner(in entity.owner, storage.entities))
                {
                    despawnRequests.Enqueue(new WeaponDespawnRequest { target = new EntityIdentifier { index = index, generation = entity.generation } });
                }
            }
        }

        // 주인의 칸에 그 주인이 아직 살아 있는지
        private static bool HasOwner(in OwnerComponent owner, Entity[] players)
        {
            ref readonly Entity player = ref players[owner.owner.index];
            return (player.mask & PLAYER_MASK) == PLAYER_MASK && player.generation == owner.owner.generation;
        }
    }
}
