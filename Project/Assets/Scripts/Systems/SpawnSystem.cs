using System;
using System.Collections.Generic;

namespace TopView.Systems
{
    // 엔티티 종류별 ~SpawnSystem이 함께 쓰는 칸 할당 도구.
    public static class SpawnSystem
    {
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
