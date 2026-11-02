using TopView.Components;
using TopView.Entities;
using UnityEngine;

namespace TopView.Requests
{
    // 근접 판정을 만들어 달라는 요청. 근접 공격이 나갈 때 플레이어 System과 적·보스 공격이 넣는다
    public struct MeleeSpawnRequest
    {
        // 공격한 엔티티
        public EntityReference owner;
        // 공격한 쪽의 진영
        public Faction faction;
        // 판정 원의 중심(월드)
        public Vector3 center;
        // 판정 원의 반지름(유닛, XZ)
        public float radius;
        // 맞을 때의 피해량
        public float damage;
        // 판정하는 시간(틱)
        public int durationTicks;
    }
}
