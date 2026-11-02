using System.Collections.Generic;
using TopView.Components;
using TopView.Entities;
using TopView.Requests;
using UnityEngine;

namespace TopView.Systems
{
    // 플레이어 생성 요청을 받아 공유 저장소의 플레이어 구간 빈 칸에 컴포넌트를 채운다. 틱 마지막에 부른다.
    public static class PlayerSpawnSystem
    {
        /// <summary>
        /// 플레이어 생성 요청을 모두 처리한다. 빈 칸이 없으면 요청을 버린다.
        /// </summary>
        /// <param name="entities">공유 저장소</param>
        /// <param name="playerSpawnRequests">플레이어 생성 요청 큐</param>
        public static void Tick(EntityStorage entities, Queue<PlayerSpawnRequest> playerSpawnRequests)
        {
            int droppedCount = 0;
            while (playerSpawnRequests.Count > 0)
            {
                PlayerSpawnRequest request = playerSpawnRequests.Dequeue();
                if (!SpawnSystem.TryTakeFreeIndex(entities.freePlayerIndices, out int index))
                {
                    droppedCount++;
                    continue;
                }

                Spawn(entities, index, in request);
            }

            if (droppedCount > 0)
            {
                Debug.LogWarning($"PlayerSpawnSystem >> 플레이어 저장소에 빈 칸이 없어 생성 요청 {droppedCount}개를 버렸다.");
            }
        }

        private static void Spawn(EntityStorage entities, int index, in PlayerSpawnRequest request)
        {
            WeaponComponent[] weapons = SpawnSystem.ReuseArray(entities.weaponLists[index].weapons, request.weapons);
            int[] currentAmmo = ReuseAmmoArray(entities.weaponSlots[index].currentAmmo, weapons.Length);
            int[] reserveAmmo = ReuseAmmoArray(entities.weaponSlots[index].reserveAmmo, weapons.Length);
            for (int weaponIndex = 0; weaponIndex < weapons.Length; weaponIndex++)
            {
                currentAmmo[weaponIndex] = weapons[weaponIndex].startingCurrentAmmo;
                reserveAmmo[weaponIndex] = weapons[weaponIndex].startingReserveAmmo;
            }

            SpawnSystem.ClaimSlot(entities, index, ComponentMask.PlayerArchetype);
            entities.playerMovements[index] = request.movement;
            entities.playerVitals[index] = request.vitals;
            entities.playerActions[index] = request.action;
            entities.factions[index] = new FactionComponent { faction = Faction.Player };
            entities.positions[index] = new PositionComponent { previous = request.position, current = request.position };
            entities.rotations[index] = new RotationComponent { previous = request.rotation, current = request.rotation };
            entities.playerActionStates[index] = new PlayerActionStateComponent { state = PlayerState.Idle };
            entities.playerMotions[index] = new PlayerMotionComponent { currentSpeed = request.movement.walkSpeed };
            entities.healths[index] = new HealthComponent { current = request.vitals.maxHealth, max = request.vitals.maxHealth };
            entities.manas[index] = new ManaComponent { current = request.vitals.maxMana, recoveryElapsedTicks = request.vitals.manaRecoveryDelayTicks };
            entities.weaponLists[index] = new WeaponListComponent { weapons = weapons };
            entities.weaponSlots[index] = new PlayerWeaponSlotsComponent
            {
                currentAmmo = currentAmmo,
                reserveAmmo = reserveAmmo,
                currentIndex = 0,
                pendingIndex = 0
            };
        }

        // 칸이 쓰던 탄약 배열을 길이가 같으면 다시 쓴다. 내용은 부르는 쪽이 모두 채운다.
        private static int[] ReuseAmmoArray(int[] slotArray, int length)
        {
            return slotArray != null && slotArray.Length == length ? slotArray : new int[length];
        }
    }
}
