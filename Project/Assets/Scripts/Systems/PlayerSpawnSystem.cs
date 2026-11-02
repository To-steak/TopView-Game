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

            slot = new PlayerEntity
            {
                isAlive = true,
                generation = slot.generation + 1,
                movement = request.movement,
                vitals = request.vitals,
                action = request.action,
                previousPosition = request.position,
                currentPosition = request.position,
                previousRotation = request.rotation,
                currentRotation = request.rotation,
                state = PlayerState.Idle,
                currentSpeed = request.movement.walkSpeed,
                health = request.vitals.maxHealth,
                mana = request.vitals.maxMana,
                manaRecoveryElapsedTicks = request.vitals.manaRecoveryDelayTicks,
                weapons = weapons,
                currentWeaponIndex = 0,
                pendingWeaponIndex = 0
            };
        }
    }
}
