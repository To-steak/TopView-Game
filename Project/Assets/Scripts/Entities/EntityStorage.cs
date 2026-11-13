using System.Collections.Generic;
using UnityEngine;

namespace TopView.Entities
{
    // 모든 엔티티를 담는 저장소. 고정 배열 하나를 종류별 구간으로 나누고, 구간마다 빈 칸 목록을 둔다.
    // 한 종류가 많이 생겨도 다른 종류의 칸을 차지하지 않는다. 칸 번호는 모든 종류가 같은 번호 공간을 쓴다.
    public class EntityStorage
    {
        // 엔티티 배열. mask가 None인 칸은 비어 있다
        public Entity[] entities;
        // 전리품 구간의 빈 칸 번호 목록. LootSpawnSystem이 끝에서부터 꺼낸다
        public List<int> freeLootIndices;
        // 근접 판정 구간의 빈 칸 번호 목록. MeleeSpawnSystem이 끝에서부터 꺼낸다
        public List<int> freeMeleeIndices;
        // 직선 투사체 구간의 빈 칸 번호 목록. ProjectileSpawnSystem이 끝에서부터 꺼낸다
        public List<int> freeStraightProjectileIndices;
        // 포물선 투사체 구간의 빈 칸 번호 목록. ProjectileSpawnSystem이 끝에서부터 꺼낸다
        public List<int> freeArcProjectileIndices;
        // 적 종류별 구간의 빈 칸 번호 목록. 번호는 적 종류 번호다. EnemySpawnSystem이 끝에서부터 꺼낸다
        public List<int>[] freeEnemyIndicesByKind;

        /// <summary>
        /// 모든 칸이 빈 저장소를 만든다. 구간은 전리품, 근접 판정, 직선 투사체, 포물선 투사체, 적 종류별 순서로 앞에서부터 놓는다.
        /// 근접 판정 구간의 칸마다 이미 맞힌 대상을 기록할 배열을, 적 구간의 칸마다 경로 배열을 할당한다.
        /// </summary>
        /// <param name="lootCapacity">전리품 구간의 칸 수</param>
        /// <param name="meleeCapacity">근접 판정 구간의 칸 수</param>
        /// <param name="meleeHitCapacity">근접 판정 하나가 기록할 수 있는 이미 맞힌 대상 수</param>
        /// <param name="straightProjectileCapacity">직선 투사체 구간의 칸 수</param>
        /// <param name="arcProjectileCapacity">포물선 투사체 구간의 칸 수</param>
        /// <param name="enemyKindCount">적 종류 수</param>
        /// <param name="enemyCapacityPerKind">적 종류별 구간의 칸 수</param>
        /// <param name="pathCornerCapacity">적 칸마다 미리 만들어 둘 경로 배열의 크기</param>
        public EntityStorage(int lootCapacity, int meleeCapacity, int meleeHitCapacity, int straightProjectileCapacity, int arcProjectileCapacity, int enemyKindCount, int enemyCapacityPerKind, int pathCornerCapacity)
        {
            entities = new Entity[lootCapacity + meleeCapacity + straightProjectileCapacity + arcProjectileCapacity + enemyKindCount * enemyCapacityPerKind];

            int start = 0;
            freeLootIndices = CreateFreeIndices(start, lootCapacity);
            start += lootCapacity;

            freeMeleeIndices = CreateFreeIndices(start, meleeCapacity);
            for (int index = start; index < start + meleeCapacity; index++)
            {
                entities[index].meleeHits.targets = new EntityIdentifier[meleeHitCapacity];
            }
            start += meleeCapacity;

            freeStraightProjectileIndices = CreateFreeIndices(start, straightProjectileCapacity);
            start += straightProjectileCapacity;

            freeArcProjectileIndices = CreateFreeIndices(start, arcProjectileCapacity);
            start += arcProjectileCapacity;

            freeEnemyIndicesByKind = new List<int>[enemyKindCount];
            for (int kind = 0; kind < enemyKindCount; kind++)
            {
                freeEnemyIndicesByKind[kind] = CreateFreeIndices(start, enemyCapacityPerKind);
                for (int index = start; index < start + enemyCapacityPerKind; index++)
                {
                    entities[index].navigationAgent.pathCorners = new Vector3[pathCornerCapacity];
                }
                start += enemyCapacityPerKind;
            }
        }

        // start부터 count칸의 번호를 끝에서부터 꺼낼 때 앞 번호가 먼저 나오도록 거꾸로 담는다.
        private static List<int> CreateFreeIndices(int start, int count)
        {
            List<int> freeIndices = new List<int>(count);
            for (int index = start + count - 1; index >= start; index--)
            {
                freeIndices.Add(index);
            }
            return freeIndices;
        }
    }
}
