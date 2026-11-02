using TopView.Entities;
using UnityEngine;

namespace TopView.Components
{
    // 이번 틱에 공격을 맞을 수 있는 대상 하나. 판정 직전에 HitTargetSystem이 플레이어·적·보스에서 모아 World의 피격 대상 목록에 채운다.
    public struct HitTargetComponent
    {
        // 맞을 엔티티
        public EntityReference target;
        // 대상의 진영
        public Faction faction;
        // 위치(월드)
        public Vector3 position;
        // 피격 판정 반지름(유닛, XZ)
        public float radius;
    }
}
