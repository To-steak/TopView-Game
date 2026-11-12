using System.Collections.Generic;
using TopView.Components;
using TopView.Entities;
using TopView.Requests;
using TopView.Utilities;
using UnityEngine;

namespace TopView.Systems
{
    // 플레이어 생성 요청을 받아 저장소의 빈 칸을 채우고, 플레이어가 들 무기마다 무기 생성 요청을 넣는다. 틱 마지막에 WeaponSpawnSystem 앞에 부른다.
    public static class PlayerSpawnSystem
    {
        /// <summary>
        /// 플레이어 생성 요청을 모두 처리한다. 빈 칸이 없으면 요청을 버린다.
        /// </summary>
        /// <param name="players">플레이어 저장소</param>
        /// <param name="playerSpawnRequests">플레이어 생성 요청 큐</param>
        public static void Tick(PlayerStorage players, Queue<PlayerSpawnRequest> playerSpawnRequests, Queue<WeaponSpawnRequest> weaponSpawnRequests)
        {
            int droppedCount = 0;
            while (playerSpawnRequests.Count > 0)
            {
                PlayerSpawnRequest request = playerSpawnRequests.Dequeue();
                if (!SpawnUtility.TryTakeFreeIndex(players.freeIndices, out int index))
                {
                    droppedCount++;
                    continue;
                }

                ref PlayerEntity entity = ref players.entities[index];
                Spawn(ref entity, in request);
                EntityIdentifier owner = new EntityIdentifier { index = index, generation = entity.generation };
                for (int weaponIndex = 0; weaponIndex < request.weapons.Length; weaponIndex++)
                {
                    weaponSpawnRequests.Enqueue(new WeaponSpawnRequest { owner = owner, slotIndex = weaponIndex, weapon = request.weapons[weaponIndex] });
                }
            }

            if (droppedCount > 0)
            {
                Debug.LogWarning($"PlayerSpawnSystem >> 플레이어 저장소에 빈 칸이 없어 생성 요청 {droppedCount}개를 버렸다.");
            }
        }

        // 칸 전체를 새 값으로 바꿔 이전 값이 남지 않게 한다. 무기 목록과 무기 식별자 배열, 탄약 배열, 투척 미리보기 점 배열, 무기별 강화 레벨 배열만 다시 쓴다.
        private static void Spawn(ref PlayerEntity entity, in PlayerSpawnRequest request)
        {
            int generation = entity.generation + 1;
            WeaponComponent[] weapons = SpawnUtility.ReuseArray(entity.weaponList.weapons, request.weapons);
            EntityIdentifier[] weaponIdentifiers = entity.weaponSlots.weapons;
            if (weaponIdentifiers == null || weaponIdentifiers.Length != weapons.Length)
            {
                weaponIdentifiers = new EntityIdentifier[weapons.Length];
            }
            System.Array.Clear(weaponIdentifiers, 0, weaponIdentifiers.Length);
            int[] currentAmmo = ReuseIntArray(entity.weaponSlots.currentAmmo, weapons.Length);
            int[] reserveAmmo = ReuseIntArray(entity.weaponSlots.reserveAmmo, weapons.Length);
            for (int weaponIndex = 0; weaponIndex < weapons.Length; weaponIndex++)
            {
                currentAmmo[weaponIndex] = weapons[weaponIndex].ammo.startingCurrent;
                reserveAmmo[weaponIndex] = weapons[weaponIndex].ammo.startingReserve;
            }
            Vector3[] previewPoints = ReusePreviewPointArray(entity.throwPreview.points, GetPreviewPointCapacity(weapons));
            int[] weaponLevels = ReuseIntArray(entity.upgradeLevels.weaponLevels, weapons.Length);
            System.Array.Clear(weaponLevels, 0, weaponLevels.Length);

            entity = new PlayerEntity
            {
                isAlive = true,
                generation = generation,
                movement = request.movement,
                vitals = request.vitals,
                action = request.action,
                upgrade = request.upgrade,
                position = new PositionComponent { previous = request.position, current = request.position },
                rotation = new RotationComponent { previous = request.rotation, current = request.rotation },
                actionState = new PlayerActionStateComponent { state = PlayerState.Idle },
                motion = new PlayerMotionComponent { currentSpeed = request.movement.walkSpeed },
                health = new HealthComponent { current = request.vitals.startingMaxHealth, max = request.vitals.startingMaxHealth },
                mana = new ManaComponent { current = request.vitals.startingMaxMana, max = request.vitals.startingMaxMana, recoveryElapsedTicks = request.vitals.manaRecoveryDelayTicks },
                weaponList = new WeaponListComponent { weapons = weapons },
                weaponSlots = new PlayerWeaponSlotsComponent
                {
                    weapons = weaponIdentifiers,
                    currentAmmo = currentAmmo,
                    reserveAmmo = reserveAmmo,
                    currentIndex = 0,
                    pendingIndex = 0
                },
                throwPreview = new PlayerThrowPreviewComponent { points = previewPoints },
                upgradeLevels = new PlayerUpgradeLevelsComponent { weaponLevels = weaponLevels }
            };
        }

        // 투척 무기 중 가장 긴 비행의 점 수(flightTicks + 1). 투척 무기가 없으면 0이다.
        private static int GetPreviewPointCapacity(WeaponComponent[] weapons)
        {
            int capacity = 0;
            for (int weaponIndex = 0; weaponIndex < weapons.Length; weaponIndex++)
            {
                ref readonly WeaponComponent weapon = ref weapons[weaponIndex];
                if (weapon.kind == WeaponKind.Throw)
                {
                    capacity = Mathf.Max(capacity, weapon.projectile.flightTicks + 1);
                }
            }

            return capacity;
        }

        // 칸이 쓰던 미리보기 점 배열을 길이가 같으면 다시 쓴다. 내용은 PlayerThrowPreviewSystem이 매 틱 채운다.
        private static Vector3[] ReusePreviewPointArray(Vector3[] slotArray, int length)
        {
            return slotArray != null && slotArray.Length == length ? slotArray : new Vector3[length];
        }

        // 칸이 쓰던 int 배열(탄약, 강화 레벨)을 길이가 같으면 다시 쓴다. 내용은 부르는 쪽이 모두 채운다.
        private static int[] ReuseIntArray(int[] slotArray, int length)
        {
            return slotArray != null && slotArray.Length == length ? slotArray : new int[length];
        }
    }
}
