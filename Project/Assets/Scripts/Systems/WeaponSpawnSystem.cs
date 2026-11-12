using System.Collections.Generic;
using TopView.Components;
using TopView.Entities;
using TopView.Requests;
using TopView.Utilities;
using UnityEngine;

namespace TopView.Systems
{
    // 무기 생성 요청을 받아 저장소의 빈 칸을 채우고, 주인의 무기 목록에 식별자를 적는다. 틱 마지막에 PlayerSpawnSystem 다음에 부른다.
    public static class WeaponSpawnSystem
    {
        /// <summary>
        /// 무기 생성 요청을 모두 처리한다. 주인이 없거나, 주인의 무기 목록에 그 칸이 없거나, 빈 칸이 없으면 요청을 버린다.
        /// </summary>
        /// <param name="weapons">무기 저장소</param>
        /// <param name="players">플레이어 저장소. 주인의 무기 목록에 식별자를 적는다</param>
        /// <param name="weaponSpawnRequests">무기 생성 요청 큐</param>
        public static void Tick(WeaponStorage weapons, PlayerStorage players, Queue<WeaponSpawnRequest> weaponSpawnRequests)
        {
            int missingOwnerCount = 0;
            int droppedCount = 0;
            while (weaponSpawnRequests.Count > 0)
            {
                WeaponSpawnRequest request = weaponSpawnRequests.Dequeue();
                ref PlayerEntity player = ref players.entities[request.owner.index];
                EntityIdentifier[] ownerWeapons = player.weaponSlots.weapons;
                if (!player.isAlive || player.generation != request.owner.generation || ownerWeapons == null || request.slotIndex < 0 || request.slotIndex >= ownerWeapons.Length)
                {
                    missingOwnerCount++;
                    continue;
                }

                if (!SpawnUtility.TryTakeFreeIndex(weapons.freeIndices, out int index))
                {
                    droppedCount++;
                    continue;
                }

                ref WeaponEntity entity = ref weapons.entities[index];
                Spawn(ref entity, in request);
                ownerWeapons[request.slotIndex] = new EntityIdentifier { index = index, generation = entity.generation };
            }

            if (missingOwnerCount > 0)
            {
                Debug.LogWarning($"WeaponSpawnSystem >> 주인이 없거나 주인의 무기 목록에 그 칸이 없어 무기 생성 요청 {missingOwnerCount}개를 버렸다.");
            }
            if (droppedCount > 0)
            {
                Debug.LogWarning($"WeaponSpawnSystem >> 무기 저장소에 빈 칸이 없어 생성 요청 {droppedCount}개를 버렸다.");
            }
        }

        // 칸 전체를 새 값으로 바꿔 이전 값이 남지 않게 한다.
        private static void Spawn(ref WeaponEntity entity, in WeaponSpawnRequest request)
        {
            int generation = entity.generation + 1;
            entity = new WeaponEntity
            {
                isAlive = true,
                generation = generation,
                owner = new OwnerComponent { owner = request.owner, slotIndex = request.slotIndex },
                weapon = request.weapon
            };
        }
    }
}
