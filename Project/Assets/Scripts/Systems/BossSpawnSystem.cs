using System.Collections.Generic;
using TopView.Components;
using TopView.Entities;
using TopView.Requests;
using UnityEngine;

namespace TopView.Systems
{
    // 보스 생성 요청을 받아 공유 저장소의 보스 구간 빈 칸에 컴포넌트를 채운다. 틱 마지막에 부른다.
    public static class BossSpawnSystem
    {
        /// <summary>
        /// 보스 생성 요청을 모두 처리한다. 빈 칸이 없으면 요청을 버린다.
        /// </summary>
        /// <param name="entities">공유 저장소</param>
        /// <param name="bossSpawnRequests">보스 생성 요청 큐</param>
        public static void Tick(EntityStorage entities, Queue<BossSpawnRequest> bossSpawnRequests)
        {
            bool hasPlayer = TargetingSystem.TryGetPlayerTarget(entities.masks, entities.positions, entities.playerMovements, out Vector3 playerPosition, out float _);
            int droppedCount = 0;
            while (bossSpawnRequests.Count > 0)
            {
                BossSpawnRequest request = bossSpawnRequests.Dequeue();
                if (!SpawnSystem.TryTakeFreeIndex(entities.freeBossIndices, out int index))
                {
                    droppedCount++;
                    continue;
                }

                Spawn(entities, index, in request, hasPlayer, playerPosition);
            }

            if (droppedCount > 0)
            {
                Debug.LogWarning($"BossSpawnSystem >> 보스 저장소에 빈 칸이 없어 생성 요청 {droppedCount}개를 버렸다.");
            }
        }

        // 보스는 도발 상태로 생기며, 도발을 시작할 때처럼 플레이어 쪽(XZ)을 바로 바라본다.
        private static void Spawn(EntityStorage entities, int index, in BossSpawnRequest request, bool hasPlayer, Vector3 playerPosition)
        {
            EnemyAttackComponent[] attacks = SpawnSystem.ReuseArray(entities.attackLists[index].attacks, request.attacks);

            Quaternion rotation = Quaternion.identity;
            Vector3 toPlayer = playerPosition - request.position;
            toPlayer.y = 0f;
            if (hasPlayer && toPlayer.sqrMagnitude > 0f)
            {
                rotation = Quaternion.LookRotation(toPlayer);
            }

            float maxHealth = request.stats.baseMaxHealth + request.level * request.stats.maxHealthIncrement;

            SpawnSystem.ClaimSlot(entities, index, ComponentMask.BossArchetype);
            entities.factions[index] = new FactionComponent { faction = Faction.Enemy };
            entities.enemyStats[index] = request.stats;
            entities.bossBehaviors[index] = request.behavior;
            entities.attackLists[index] = new AttackListComponent { attacks = attacks };
            entities.combats[index] = new CombatComponent
            {
                level = request.level,
                attackDamage = request.stats.baseAttackDamage + request.level * request.stats.attackDamageIncrement
            };
            entities.healths[index] = new HealthComponent { current = maxHealth, max = maxHealth };
            entities.positions[index] = new PositionComponent { previous = request.position, current = request.position };
            entities.rotations[index] = new RotationComponent { previous = rotation, current = rotation };
            entities.bossActionStates[index] = new BossActionStateComponent { state = BossState.Taunt };
        }
    }
}
