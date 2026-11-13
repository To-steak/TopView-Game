using System.Collections.Generic;

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

        /// <summary>
        /// 모든 칸이 빈 저장소를 만든다. 구간은 전리품 순서로 앞에서부터 놓는다.
        /// </summary>
        /// <param name="lootCapacity">전리품 구간의 칸 수</param>
        public EntityStorage(int lootCapacity)
        {
            entities = new Entity[lootCapacity];
            freeLootIndices = CreateFreeIndices(0, lootCapacity);
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
