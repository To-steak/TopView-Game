using System.Collections.Generic;
using TopView.Components;
using TopView.Entities;
using TopView.Requests;
using UnityEngine;

namespace TopView.Systems
{
    // 보스 생성 요청을 받아 BossStorage의 빈 칸에 엔티티를 채운다. 틱 마지막에 부른다.
    public static class BossSpawnSystem
    {
        /// <summary>
        /// 보스 생성 요청을 모두 처리한다. 빈 칸이 없으면 요청을 버린다.
        /// </summary>
        /// <param name="bosses">보스 저장소</param>
        /// <param name="bossSpawnRequests">보스 생성 요청 큐</param>
        /// <param name="players">플레이어 저장소. 보스가 생길 때 바라볼 방향을 정한다</param>
        public static void Tick(BossStorage bosses, Queue<BossSpawnRequest> bossSpawnRequests, PlayerStorage players)
        {
            bool hasPlayer = TargetingSystem.TryGetPlayerTarget(players, out Vector3 playerPosition, out float _);
            int droppedCount = 0;
            while (bossSpawnRequests.Count > 0)
            {
                BossSpawnRequest request = bossSpawnRequests.Dequeue();
                if (!SpawnSystem.TryTakeFreeIndex(bosses.freeIndices, out int index))
                {
                    droppedCount++;
                    continue;
                }

                Spawn(ref bosses.entities[index], in request, hasPlayer, playerPosition);
            }

            if (droppedCount > 0)
            {
                Debug.LogWarning($"BossSpawnSystem >> 보스 저장소에 빈 칸이 없어 생성 요청 {droppedCount}개를 버렸다.");
            }
        }

        // 보스는 도발 상태로 생기며, 도발을 시작할 때처럼 플레이어 쪽(XZ)을 바로 바라본다.
        private static void Spawn(ref BossEntity slot, in BossSpawnRequest request, bool hasPlayer, Vector3 playerPosition)
        {
            EnemyAttackComponent[] attacks = SpawnSystem.ReuseArray(slot.attacks, request.attacks);

            Quaternion rotation = Quaternion.identity;
            Vector3 toPlayer = playerPosition - request.position;
            toPlayer.y = 0f;
            if (hasPlayer && toPlayer.sqrMagnitude > 0f)
            {
                rotation = Quaternion.LookRotation(toPlayer);
            }

            float maxHealth = request.stats.basicMaxHealth + request.level * request.stats.levelIncrement;
            slot = new BossEntity
            {
                isAlive = true,
                generation = slot.generation + 1,
                stats = request.stats,
                behavior = request.behavior,
                attacks = attacks,
                level = request.level,
                maxHealth = maxHealth,
                health = maxHealth,
                attackDamage = request.stats.basicAttackDamage + request.level * request.stats.levelIncrement,
                previousPosition = request.position,
                currentPosition = request.position,
                previousRotation = rotation,
                currentRotation = rotation,
                state = BossState.Taunt
            };
        }
    }
}
