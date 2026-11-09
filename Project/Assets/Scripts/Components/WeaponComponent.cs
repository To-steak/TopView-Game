using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace TopView.Components
{
    // 무기 하나의 값. 생성 때 Definition에서 복사하며, 게임 중의 탄약은 플레이어 엔티티에 있다.
    [Serializable]
    public struct WeaponComponent
    {
        // 무기 종류
        public WeaponKind kind;
        // 한 번 맞을 때의 피해량
        public float damage;
        // 장전할 수 있는 최대 탄약 수. Ranged만 쓴다
        public int maxAmmo;
        // 시작할 때 장전된 탄약 수. Ranged만 쓴다
        [FormerlySerializedAs("currentAmmo")]
        public int startingCurrentAmmo;
        // 시작할 때 예비 탄약 수. Ranged와 Throw가 쓴다
        [FormerlySerializedAs("reserveAmmo")]
        public int startingReserveAmmo;
        // 공격 상태에 들어간 뒤 공격이 나가기까지의 시간(틱). Melee는 판정이 시작되고, Ranged와 Throw는 투사체를 내보낸다
        [FormerlySerializedAs("attackCommitTicks")]
        public int attackStartTicks;
        // 공격 상태에 들어간 뒤 근접 판정이 끝나기까지의 시간(틱). attackStartTicks보다 커야 한다. Melee만 쓴다
        public int attackEndTicks;
        // 공격 상태에 들어간 뒤 공격 상태가 끝나기까지의 시간(틱). Melee는 attackEndTicks 이후가 판정 없는 후딜레이다. Melee와 Throw가 쓴다
        public int attackDurationTicks;
        // 공격 상태에 들어간 뒤 다음 공격 상태에 들어갈 수 있기까지의 시간(틱). 공격 상태가 끝나는 시간보다 짧으면 효과가 없다. Ranged와 Melee가 쓴다
        public int attackIntervalTicks;
        // 내보내는 투사체. Ranged와 Throw가 쓴다
        public ProjectileComponent projectile;
        // 근접 판정 구의 반지름(유닛, XZ). Melee만 쓴다
        public float meleeRadius;
        // 근접 판정 구 중심을 플레이어 앞쪽으로 띄우는 거리(유닛). Melee만 쓴다
        public float meleeOffset;
        // 투사체가 생기는 위치. 플레이어 회전 기준의 로컬 위치. Ranged와 Throw가 쓰며, 조준 평면의 높이는 모든 종류가 이 값의 높이를 쓴다
        public Vector3 muzzleOffset;
        // 근접 판정이 시작될 때 재생할 피격 이펙트 표의 번호. Melee만 쓴다. 판을 준비할 때 Core가 채우며, 음수면 없음
        [NonSerialized] public int impactEffectIndex;
    }
}
