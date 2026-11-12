using System;
using UnityEngine;

namespace TopView.Components
{
    // 무기 하나의 값. 생성 때 Definition에서 복사하며, 게임 중의 탄약은 무기 엔티티에, 강화 레벨은 플레이어 엔티티에 있다.
    [Serializable]
    public struct WeaponComponent
    {
        // 무기 종류
        public WeaponKind kind;
        // 한 번 맞을 때의 피해량
        public float damage;
        // 탄약 값. Ranged와 Throw가 쓴다
        public AmmoComponent ammo;
        // 공격 상태의 시간 구간. startTicks에 Melee는 판정이 시작되고, Ranged와 Throw는 투사체를 내보낸다. endTicks는 Melee만, durationTicks는 Melee와 Throw가 쓴다
        public AttackTimingComponent timing;
        // 공격 상태에 들어간 뒤 다음 공격 상태에 들어갈 수 있기까지의 시간(틱). 공격 상태가 끝나는 시간보다 짧으면 효과가 없다. Ranged와 Melee가 쓴다
        public int attackIntervalTicks;
        // 내보내는 투사체. Ranged와 Throw가 쓴다
        public ProjectileComponent projectile;
        // 던지기 전에 착탄점을 정하는 값. Throw만 쓴다
        public ThrowComponent throwing;
        // 근접 판정 구의 크기와 위치. Melee만 쓴다
        public MeleeShapeComponent meleeShape;
        // 강화할 때 레벨마다 붙는 효과
        public WeaponUpgradeRuleComponent upgrade;
        // 투사체가 생기는 위치. 플레이어 회전 기준의 로컬 위치. Ranged와 Throw가 쓰며, 조준 평면의 높이는 모든 종류가 이 값의 높이를 쓴다
        public Vector3 muzzleOffset;
        // 근접 판정이 시작될 때 재생할 피격 이펙트 표의 번호. Melee만 쓴다. 판을 준비할 때 Core가 채우며, 음수면 없음
        [NonSerialized] public int impactEffectIndex;
    }
}
