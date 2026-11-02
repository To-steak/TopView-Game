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
        // Straight 투사체가 맞는 높이의 아래 끝(월드 Y). 높이를 따지지 않는 대상은 음의 무한대
        public float bottomHeight;
        // Straight 투사체가 맞는 높이의 위 끝(월드 Y). 높이를 따지지 않는 대상은 양의 무한대
        public float topHeight;
    }
}
