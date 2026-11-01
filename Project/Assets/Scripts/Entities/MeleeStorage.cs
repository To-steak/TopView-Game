using System.Collections.Generic;

namespace TopView.Entities
{
    // 근접 판정 엔티티 저장소. 고정 배열과 빈 칸 목록으로 보관한다.
    public class MeleeStorage
    {
        // 엔티티 배열. isAlive가 false인 칸은 비어 있다
        public MeleeEntity[] entities;
        // 빈 칸 번호 목록. MeleeSpawnSystem이 끝에서부터 꺼낸다
        public List<int> freeIndices;

        /// <summary>
        /// 모든 칸이 빈 저장소를 만들고, 칸마다 이미 맞힌 대상을 기록할 배열을 할당한다.
        /// </summary>
        /// <param name="capacity">칸 수</param>
        /// <param name="hitCapacity">칸 하나가 기록할 수 있는 이미 맞힌 대상 수</param>
        public MeleeStorage(int capacity, int hitCapacity)
        {
            entities = new MeleeEntity[capacity];
            freeIndices = new List<int>(capacity);
            for (int index = capacity - 1; index >= 0; index--)
            {
                entities[index].hitTargets = new EntityReference[hitCapacity];
                freeIndices.Add(index);
            }
        }
    }
}
