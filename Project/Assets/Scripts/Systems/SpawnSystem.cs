using System;
using System.Collections.Generic;
using TopView.Entities;

namespace TopView.Systems
{
    // 엔티티 종류별 ~SpawnSystem이 함께 쓰는 칸 할당 도구.
    public static class SpawnSystem
    {
        /// <summary>
        /// 공유 저장소의 칸을 새 엔티티에 내준다. 세대를 올리고 모든 컴포넌트를 비운 뒤 표시를 정한다. 컴포넌트 배열을 추가하면 여기서도 비운다.
        /// </summary>
        /// <param name="entities">공유 저장소</param>
        /// <param name="index">내줄 칸 번호</param>
        /// <param name="mask">새 엔티티가 가질 컴포넌트 표시</param>
        public static void ClaimSlot(EntityStorage entities, int index, ComponentMask mask)
        {
            entities.generations[index]++;
            entities.masks[index] = mask;
            entities.positions[index] = default;
            entities.rotations[index] = default;
            entities.projectiles[index] = default;
            entities.projectileMotions[index] = default;
            entities.attackPayloads[index] = default;
            entities.melees[index] = default;
            entities.meleeHits[index] = default;
            entities.healths[index] = default;
            entities.combats[index] = default;
            entities.enemyStats[index] = default;
            entities.attackLists[index] = default;
            entities.bossBehaviors[index] = default;
            entities.bossActionStates[index] = default;
        }

        /// <summary>
        /// 공유 저장소의 엔티티를 즉시 없애고 칸을 반납한다.
        /// </summary>
        /// <param name="masks">칸별 컴포넌트 표시</param>
        /// <param name="freeIndices">그 엔티티 종류 구간의 빈 칸 번호 목록</param>
        /// <param name="index">없앨 칸 번호</param>
        public static void ReleaseSlot(ComponentMask[] masks, List<int> freeIndices, int index)
        {
            masks[index] = ComponentMask.None;
            freeIndices.Add(index);
        }

        /// <summary>
        /// 빈 칸 번호 목록의 끝에서 하나를 꺼낸다.
        /// </summary>
        /// <param name="freeIndices">빈 칸 번호 목록</param>
        /// <param name="index">꺼낸 칸 번호. 빈 칸이 없으면 -1</param>
        /// <returns>빈 칸이 있었으면 true</returns>
        public static bool TryTakeFreeIndex(List<int> freeIndices, out int index)
        {
            if (freeIndices.Count == 0)
            {
                index = -1;
                return false;
            }

            index = freeIndices[freeIndices.Count - 1];
            freeIndices.RemoveAt(freeIndices.Count - 1);
            return true;
        }

        /// <summary>
        /// 칸이 쓰던 배열을 길이가 같으면 다시 쓰고, 비운 뒤 요청의 내용을 복사한다.
        /// </summary>
        /// <param name="slotArray">칸이 쓰던 배열. null일 수 있다</param>
        /// <param name="requestArray">요청에 담긴 배열</param>
        /// <returns>요청의 내용이 담긴 배열</returns>
        public static T[] ReuseArray<T>(T[] slotArray, T[] requestArray)
        {
            T[] array = slotArray;
            if (array == null || array.Length != requestArray.Length)
            {
                array = new T[requestArray.Length];
            }
            Array.Clear(array, 0, array.Length);
            Array.Copy(requestArray, array, requestArray.Length);
            return array;
        }
    }
}
