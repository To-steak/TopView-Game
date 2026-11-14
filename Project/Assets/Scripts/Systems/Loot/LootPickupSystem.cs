using System.Collections.Generic;
using TopView.Components;
using TopView.Entities;
using TopView.Records;
using TopView.Requests;
using UnityEngine;

namespace TopView.Systems
{
    // 플레이어가 반경 안에 들어온 전리품을 획득해 효과를 적용한다.
    public static class LootPickupSystem
    {
        // 탄약 전리품을 받는 무기가 가진 컴포넌트
        private const ComponentMask AMMO_LOOT_MASK = ComponentMask.Magazine | ComponentMask.ReserveAmmoState;
        // 획득할 수 있는 전리품이 가진 컴포넌트
        private const ComponentMask LOOT_MASK = ComponentMask.Loot | ComponentMask.Position;

        // 처리할 플레이어가 가진 컴포넌트
        private const ComponentMask PLAYER_MASK = ComponentMask.PlayerActionState | ComponentMask.Position | ComponentMask.Health | ComponentMask.WeaponSlots;

        /// <summary>
        /// 살아 있는 플레이어의 획득 반경 안에 있는 전리품의 효과를 적용하고 소멸을 요청한다. 효과를 받을 수 없는 전리품(원거리 무기가 없을 때의 탄약)은 바닥에 남긴다.
        /// </summary>
        /// <param name="storage">엔티티 저장소. 플레이어(PlayerActionState, Position, Health, WeaponSlots)와 전리품(Loot, Position)을 처리하고, 플레이어의 health와 탄약을 받는 무기(Magazine, ReserveAmmoState)의 reserveAmmoState를 고친다</param>
        /// <param name="progress">한 판의 진행 상태. 골드를 더한다</param>
        /// <param name="pickupRadius">플레이어 중심에서 전리품을 획득하는 반경(유닛, XZ)</param>
        /// <param name="despawnRequests">전리품 소멸 요청 큐</param>
        /// <param name="pickedUpRecords">전리품을 획득한 기록 목록</param>
        public static void Tick(EntityStorage storage, ref GameProgressComponent progress, float pickupRadius, Queue<LootDespawnRequest> despawnRequests, List<LootPickedUpRecord> pickedUpRecords)
        {
            float sqrPickupRadius = pickupRadius * pickupRadius;
            for (int playerIndex = 0; playerIndex < storage.entities.Length; playerIndex++)
            {
                ref Entity player = ref storage.entities[playerIndex];
                if ((player.mask & PLAYER_MASK) != PLAYER_MASK)
                {
                    continue;
                }

                for (int lootIndex = 0; lootIndex < storage.entities.Length; lootIndex++)
                {
                    ref readonly Entity entity = ref storage.entities[lootIndex];
                    // 체력이 0인 플레이어는 회복으로 되살아나지 않도록 매번 확인한다
                    if ((entity.mask & LOOT_MASK) != LOOT_MASK || player.health.current <= 0f)
                    {
                        continue;
                    }

                    Vector3 offset = entity.position.current - player.position.current;
                    offset.y = 0f;
                    if (offset.sqrMagnitude > sqrPickupRadius)
                    {
                        continue;
                    }

                    if (TryApply(in entity.loot, ref progress, ref player.health, storage.entities, in player.weaponSlots))
                    {
                        despawnRequests.Enqueue(new LootDespawnRequest
                        {
                            target = new EntityIdentifier { index = lootIndex, generation = entity.generation }
                        });
                        pickedUpRecords.Add(new LootPickedUpRecord { kind = entity.loot.kind, amount = entity.loot.amount, position = entity.position.current });
                    }
                }
            }
        }

        // 전리품의 효과를 적용한다. 효과를 받을 수 없어 바닥에 남길 전리품이면 false를 돌려준다.
        private static bool TryApply(in LootComponent loot, ref GameProgressComponent progress, ref HealthComponent health, Entity[] weaponEntities, in PlayerWeaponSlotsComponent weaponSlots)
        {
            switch (loot.kind)
            {
                case LootKind.Gold:
                    progress.gold += loot.amount;
                    return true;
                case LootKind.Health:
                    // 체력이 가득 차 있어도 획득한다. 남기면 처음 하는 사람이 못 줍는 버그로 오해한다
                    health.current = Mathf.Min(health.max, health.current + loot.amount);
                    return true;
                case LootKind.Ammo:
                    return TryAddAmmo(weaponEntities, in weaponSlots, loot.amount);
                default:
                    // 없앤 뒤에는 다시 들어오지 않으므로 틱마다 반복되지 않는다
                    Debug.LogError($"LootPickupSystem >> 효과가 없는 전리품 종류 {loot.kind}를 효과 없이 없앤다.");
                    return true;
            }
        }

        // 탄창이 있는 무기 전부의 예비 탄약을 더한다. 그런 무기가 없으면 false를 돌려준다.
        private static bool TryAddAmmo(Entity[] weaponEntities, in PlayerWeaponSlotsComponent weaponSlots, int amount)
        {
            bool hasRangedWeapon = false;
            for (int weaponIndex = 0; weaponIndex < weaponSlots.weapons.Length; weaponIndex++)
            {
                ref Entity weaponEntity = ref weaponEntities[weaponSlots.weapons[weaponIndex].index];
                if ((weaponEntity.mask & AMMO_LOOT_MASK) == AMMO_LOOT_MASK)
                {
                    weaponEntity.reserveAmmoState.reserve += amount;
                    hasRangedWeapon = true;
                }
            }

            return hasRangedWeapon;
        }
    }
}
