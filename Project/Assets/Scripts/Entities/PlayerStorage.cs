using System.Collections.Generic;

namespace TopView.Entities
{
    // 플레이어 엔티티 저장소. 고정 배열과 빈 칸 목록으로 보관한다.
    public class PlayerStorage
    {
        // 엔티티 배열. isAlive가 false인 칸은 비어 있다
        public PlayerEntity[] entities;
        // 빈 칸 번호 목록. PlayerSpawnSystem이 끝에서부터 꺼낸다
        public List<int> freeIndices;

        /// <summary>
        /// 모든 칸이 빈 저장소를 만든다.
        /// </summary>
        /// <param name="capacity">칸 수</param>
        public PlayerStorage(int capacity)
        {
            entities = new PlayerEntity[capacity];
            freeIndices = new List<int>(capacity);
            for (int index = capacity - 1; index >= 0; index--)
            {
                freeIndices.Add(index);
            }
        }
    }
}
