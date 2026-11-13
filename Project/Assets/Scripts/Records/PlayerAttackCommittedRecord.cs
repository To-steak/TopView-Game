using TopView.Entities;
using UnityEngine;

namespace TopView.Records
{
    // 플레이어가 공격을 내보냈다. 원거리는 탄을 쏘았을 때, 투척은 던졌을 때, 근접은 판정이 시작될 때 남는다
    public struct PlayerAttackCommittedRecord
    {
        // 공격한 플레이어
        public EntityIdentifier player;
        // 공격에 쓴 무기 번호
        public int weaponIndex;
        // 임팩트 이펙트를 재생할 위치(월드). 근접 판정 원의 중심이다. 근접 공격만 쓴다
        public Vector3 impactPosition;
        // 임팩트 이펙트의 피격 이펙트 표 번호. 음수면 없음. 근접 공격이 아니면 항상 음수다
        public int impactEffectIndex;
        // 임팩트 이펙트 크기 배율. 근접 범위 강화가 반영된다. 근접 공격이 아니면 1이다
        public float impactScale;
    }
}
