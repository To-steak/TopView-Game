using System.Collections.Generic;

namespace TopView.Entities
{
    // 적 엔티티 저장소. 종류마다 같은 칸 수의 구간을 나눠 하나의 고정 배열에 보관한다. 종류 k의 칸은 k × capacityPerKind부터 시작한다.
    public class EnemyStorage
    {
        // 엔티티 배열. isAlive가 false인 칸은 비어 있다
        public EnemyEntity[] entities;
        // 종류별 칸 수
        public int capacityPerKind;
        // 종류별 빈 칸 번호 목록. SpawnSystem이 끝에서부터 꺼낸다
        public List<int>[] freeIndicesByKind;

        /// <summary>
        /// 모든 칸이 빈 저장소를 만든다.
        /// </summary>
        /// <param name="kindCount">적 종류 수</param>
        /// <param name="capacityPerKind">종류별 칸 수</param>
        public EnemyStorage(int kindCount, int capacityPerKind)
        {
            this.capacityPerKind = capacityPerKind;
            entities = new EnemyEntity[kindCount * capacityPerKind];
            freeIndicesByKind = new List<int>[kindCount];
            for (int kind = 0; kind < kindCount; kind++)
            {
                freeIndicesByKind[kind] = new List<int>(capacityPerKind);
                for (int index = (kind + 1) * capacityPerKind - 1; index >= kind * capacityPerKind; index--)
                {
                    freeIndicesByKind[kind].Add(index);
                }
            }
        }
    }
}
