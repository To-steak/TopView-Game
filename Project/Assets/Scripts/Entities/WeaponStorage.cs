using System.Collections.Generic;

namespace TopView.Entities
{
    // 무기 엔티티 저장소. 고정 배열과 빈 칸 목록으로 보관한다.
    public class WeaponStorage
    {
        // 엔티티 배열. isAlive가 false인 칸은 비어 있다
        public WeaponEntity[] entities;
        // 빈 칸 번호 목록. WeaponSpawnSystem이 끝에서부터 꺼낸다
        public List<int> freeIndices;

        public WeaponStorage(int capacity)
        {
            entities = new WeaponEntity[capacity];
            freeIndices = new List<int>(capacity);
            for (int index = capacity - 1; index >= 0; index--)
            {
                freeIndices.Add(index);
            }
        }
    }
}
