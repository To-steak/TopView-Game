using TopView.Components;
using UnityEngine;

namespace TopView.Entities
{
    // 근접 판정 하나의 데이터. 공격이 나갈 때 생기고, 수명 동안 범위 안의 대상을 한 번씩 맞힌 뒤 사라진다
    public struct MeleeEntity
    {
        // 살아 있는 칸인지. false면 빈 칸이다
        public bool isAlive;
        // 이 칸이 채워진 횟수
        public int generation;
        // 공격한 엔티티
        public EntityReference owner;
        // 공격한 쪽의 진영. 진영이 다른 대상만 맞힌다
        public Faction faction;
        // 판정 원의 중심(월드). 생길 때 정하며 바뀌지 않는다
        public Vector3 center;
        // 판정 원의 반지름(유닛, XZ)
        public float radius;
        // 맞을 때의 피해량
        public float damage;
        // 판정하는 시간(틱). 1 이하이면 한 틱만 판정한다
        public int durationTicks;
        // 판정을 시작한 뒤 지난 시간(틱)
        public int elapsedTicks;
        // 이미 맞힌 대상. 저장소를 만들 때 할당해 다시 쓰는 배열이며 hitTargetCount까지만 쓴다
        public EntityReference[] hitTargets;
        // 이미 맞힌 대상 수
        public int hitTargetCount;
    }
}
