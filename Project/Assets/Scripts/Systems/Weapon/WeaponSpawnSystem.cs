using System.Collections.Generic;
using TopView.Components;
using TopView.Entities;
using TopView.Requests;
using TopView.Utilities;
using UnityEngine;

namespace TopView.Systems
{
    // 무기 생성 요청을 받아 저장소 무기 구간의 빈 칸을 채우고, 주인의 무기 목록에 식별자를 적는다. 틱 마지막에 PlayerSpawnSystem 다음에 부른다.
    public static class WeaponSpawnSystem
    {
        // 처리할 플레이어가 가진 컴포넌트
        private const ComponentMask PLAYER_MASK = ComponentMask.PlayerActionState | ComponentMask.WeaponSlots;

        /// <summary>
        /// 무기 생성 요청을 모두 처리한다. 주인이 없거나, 주인의 무기 목록에 그 칸이 없거나, 빈 칸이 없으면 요청을 버린다.
        /// </summary>
        /// <param name="storage">엔티티 저장소. 무기 구간의 칸을 요청의 마스크대로 채우고, 주인 플레이어(PlayerActionState, WeaponSlots)의 무기 목록에 식별자를 적는다</param>
        /// <param name="weaponSpawnRequests">무기 생성 요청 큐</param>
        public static void Tick(EntityStorage storage, Queue<WeaponSpawnRequest> weaponSpawnRequests)
        {
            int missingOwnerCount = 0;
            int droppedCount = 0;
            while (weaponSpawnRequests.Count > 0)
            {
                WeaponSpawnRequest request = weaponSpawnRequests.Dequeue();
                ref Entity player = ref storage.entities[request.owner.index];
                EntityIdentifier[] ownerWeapons = player.weaponSlots.weapons;
                if ((player.mask & PLAYER_MASK) != PLAYER_MASK || player.generation != request.owner.generation || ownerWeapons == null || request.slotIndex < 0 || request.slotIndex >= ownerWeapons.Length)
                {
                    missingOwnerCount++;
                    continue;
                }

                if (!SpawnUtility.TryTakeFreeIndex(storage.freeWeaponIndices, out int index))
                {
                    droppedCount++;
                    continue;
                }

                ref Entity entity = ref storage.entities[index];
                Spawn(ref entity, in request);
                ownerWeapons[request.slotIndex] = new EntityIdentifier { index = index, generation = entity.generation };
            }

            if (missingOwnerCount > 0)
            {
                Debug.LogWarning($"WeaponSpawnSystem >> 주인이 없거나 주인의 무기 목록에 그 칸이 없어 무기 생성 요청 {missingOwnerCount}개를 버렸다.");
            }
            if (droppedCount > 0)
            {
                Debug.LogWarning($"WeaponSpawnSystem >> 무기 구간에 빈 칸이 없어 생성 요청 {droppedCount}개를 버렸다.");
            }
        }

        // 칸 전체를 새 값으로 바꿔 이전 값이 남지 않게 한다. 마스크에 없는 컴포넌트의 값도 요청에서 옮기지만 읽히지 않는다.
        private static void Spawn(ref Entity entity, in WeaponSpawnRequest request)
        {
            int generation = entity.generation + 1;
            entity = new Entity
            {
                mask = request.mask,
                generation = generation,
                owner = new OwnerComponent { owner = request.owner, slotIndex = request.slotIndex },
                weapon = request.weapon,
                weaponUpgrade = request.upgrade,
                attackState = request.attackState,
                meleeAttack = request.meleeAttack,
                meleeView = request.meleeView,
                straightAttack = request.straightAttack,
                arcAttack = request.arcAttack,
                throwing = request.throwing,
                projectileView = request.projectileView,
                magazine = request.magazine,
                magazineState = new MagazineStateComponent { current = request.magazine.startingCurrent },
                reserveAmmo = request.reserveAmmo,
                reserveAmmoState = new ReserveAmmoStateComponent { reserve = request.reserveAmmo.startingReserve },
                attackInterval = request.attackInterval,
                attackDuration = request.attackDuration
            };
        }
    }
}
