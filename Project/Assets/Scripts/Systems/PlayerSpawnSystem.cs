using System.Collections.Generic;
using TopView.Components;
using TopView.Entities;
using TopView.Requests;
using UnityEngine;

namespace TopView.Systems
{
    // 플레이어 생성 요청을 받아 PlayerStorage의 빈 칸에 엔티티를 채운다. 틱 마지막에 부른다.
    public static class PlayerSpawnSystem
    {
        /// <summary>
        /// 플레이어 생성 요청을 모두 처리한다. 빈 칸이 없으면 요청을 버린다.
        /// </summary>
        /// <param name="players">플레이어 저장소</param>
        /// <param name="playerSpawnRequests">플레이어 생성 요청 큐</param>
        public static void Tick(PlayerStorage players, Queue<PlayerSpawnRequest> playerSpawnRequests)
        {
            int droppedCount = 0;
            while (playerSpawnRequests.Count > 0)
            {
                PlayerSpawnRequest request = playerSpawnRequests.Dequeue();
                if (!SpawnSystem.TryTakeFreeIndex(players.freeIndices, out int index))
                {
                    droppedCount++;
                    continue;
                }

                Spawn(ref players.entities[index], in request);
            }

            if (droppedCount > 0)
            {
                Debug.LogWarning($"PlayerSpawnSystem >> 플레이어 저장소에 빈 칸이 없어 생성 요청 {droppedCount}개를 버렸다.");
            }
        }

        private static void Spawn(ref PlayerEntity slot, in PlayerSpawnRequest request)
        {
            WeaponComponent[] weapons = SpawnSystem.ReuseArray(slot.weapons, request.weapons);
            int[] weaponCurrentAmmo = ReuseAmmoArray(slot.weaponSlots.currentAmmo, weapons.Length);
            int[] weaponReserveAmmo = ReuseAmmoArray(slot.weaponSlots.reserveAmmo, weapons.Length);
            for (int weaponIndex = 0; weaponIndex < weapons.Length; weaponIndex++)
            {
                weaponCurrentAmmo[weaponIndex] = weapons[weaponIndex].startingCurrentAmmo;
                weaponReserveAmmo[weaponIndex] = weapons[weaponIndex].startingReserveAmmo;
            }

            slot = new PlayerEntity
            {
                isAlive = true,
                generation = slot.generation + 1,
                movement = request.movement,
                vitals = request.vitals,
                action = request.action,
                position = new PositionComponent { previous = request.position, current = request.position },
                rotation = new RotationComponent { previous = request.rotation, current = request.rotation },
                actionState = new PlayerActionStateComponent { state = PlayerState.Idle },
                motion = new PlayerMotionComponent { currentSpeed = request.movement.walkSpeed },
                health = new HealthComponent { current = request.vitals.maxHealth, max = request.vitals.maxHealth },
                mana = new ManaComponent { current = request.vitals.maxMana, recoveryElapsedTicks = request.vitals.manaRecoveryDelayTicks },
                weapons = weapons,
                weaponSlots = new PlayerWeaponSlotsComponent
                {
                    currentAmmo = weaponCurrentAmmo,
                    reserveAmmo = weaponReserveAmmo,
                    currentIndex = 0,
                    pendingIndex = 0
                }
            };
        }

        // 칸이 쓰던 탄약 배열을 길이가 같으면 다시 쓴다. 내용은 부르는 쪽이 모두 채운다.
        private static int[] ReuseAmmoArray(int[] slotArray, int length)
        {
            return slotArray != null && slotArray.Length == length ? slotArray : new int[length];
        }
    }
}
