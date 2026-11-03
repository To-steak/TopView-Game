using System.Collections.Generic;
using TopView.Components;
using TopView.Entities;
using TopView.Requests;
using UnityEngine;

namespace TopView.Systems
{
    // 플레이어가 반경 안에 들어온 전리품을 획득해 효과를 적용한다.
    public static class LootPickupSystem
    {
        /// <summary>
        /// 살아 있는 플레이어의 획득 반경 안에 있는 전리품의 효과를 적용하고 소멸을 요청한다. 효과를 받을 수 없는 전리품은 바닥에 남긴다.
        /// </summary>
        /// <param name="loots">전리품 저장소</param>
        /// <param name="players">플레이어 저장소</param>
        /// <param name="progress">한 판의 진행 상태. 골드를 더한다</param>
        /// <param name="pickupRadius">플레이어 중심에서 전리품을 획득하는 반경(유닛, XZ)</param>
        /// <param name="despawnRequests">전리품 소멸 요청 큐</param>
        public static void Tick(LootStorage loots, PlayerStorage players, ref GameProgressComponent progress, float pickupRadius, Queue<LootDespawnRequest> despawnRequests)
        {
            float sqrPickupRadius = pickupRadius * pickupRadius;
            for (int playerIndex = 0; playerIndex < players.entities.Length; playerIndex++)
            {
                ref PlayerEntity player = ref players.entities[playerIndex];
                if (!player.isAlive)
                {
                    continue;
                }

                for (int lootIndex = 0; lootIndex < loots.entities.Length; lootIndex++)
                {
                    ref readonly LootEntity entity = ref loots.entities[lootIndex];
                    // 체력이 0인 플레이어는 회복으로 되살아나지 않도록 매번 확인한다
                    if (!entity.isAlive || player.health.current <= 0f)
                    {
                        continue;
                    }

                    Vector3 offset = entity.position.current - player.position.current;
                    offset.y = 0f;
                    if (offset.sqrMagnitude > sqrPickupRadius)
                    {
                        continue;
                    }

                    if (TryApply(in entity.loot, ref progress, ref player.health, in player.weaponList, ref player.weaponSlots))
                    {
                        despawnRequests.Enqueue(new LootDespawnRequest
                        {
                            target = new EntityIdentifier { index = lootIndex, generation = entity.generation }
                        });
                    }
                }
            }
        }

        // 전리품의 효과를 적용한다. 효과를 받을 수 없어 바닥에 남길 전리품이면 false를 돌려준다.
        private static bool TryApply(in LootComponent loot, ref GameProgressComponent progress, ref HealthComponent health, in WeaponListComponent weaponList, ref PlayerWeaponSlotsComponent weaponSlots)
        {
            switch (loot.kind)
            {
                case LootKind.Gold:
                    progress.gold += loot.amount;
                    return true;
                case LootKind.Health:
                    return TryHeal(ref health, loot.amount);
                case LootKind.Ammo:
                    return TryAddAmmo(in weaponList, ref weaponSlots, loot.amount);
                default:
                    // 없앤 뒤에는 다시 들어오지 않으므로 틱마다 반복되지 않는다
                    Debug.LogError($"LootPickupSystem >> 효과가 없는 전리품 종류 {loot.kind}를 효과 없이 없앤다.");
                    return true;
            }
        }

        // 체력이 가득 차 있으면 회복하지 않고 false를 돌려준다.
        private static bool TryHeal(ref HealthComponent health, int amount)
        {
            if (health.current >= health.max)
            {
                return false;
            }

            health.current = Mathf.Min(health.max, health.current + amount);
            return true;
        }

        // 원거리 무기 전부의 예비 탄약을 더한다. 원거리 무기가 없으면 false를 돌려준다.
        private static bool TryAddAmmo(in WeaponListComponent weaponList, ref PlayerWeaponSlotsComponent weaponSlots, int amount)
        {
            bool hasRangedWeapon = false;
            for (int weaponIndex = 0; weaponIndex < weaponList.weapons.Length; weaponIndex++)
            {
                if (weaponList.weapons[weaponIndex].kind == WeaponKind.Ranged)
                {
                    weaponSlots.reserveAmmo[weaponIndex] += amount;
                    hasRangedWeapon = true;
                }
            }

            return hasRangedWeapon;
        }
    }
}
