using System;
using UnityEngine.Serialization;

namespace TopView.Components
{
    // 무기 하나의 값. Definition에서는 처음 값이고, 플레이어 엔티티에 복사된 뒤에는 탄약이 바뀐다.
    [Serializable]
    public struct WeaponComponent
    {
        // 무기 종류
        public WeaponKind kind;
        // 한 번 맞을 때의 피해량
        public float damage;
        // 장전할 수 있는 최대 탄약 수. Ranged만 쓴다
        public int maxAmmo;
        // 장전된 탄약 수. Ranged만 쓴다
        public int currentAmmo;
        // 예비 탄약 수. Ranged와 Throw가 쓴다
        public int reserveAmmo;
        // 공격 상태에 들어간 뒤 공격이 나가기까지의 시간(틱). Melee는 판정이 시작되고, Ranged와 Throw는 투사체를 내보낸 뒤 공격 상태가 끝난다
        [FormerlySerializedAs("attackCommitTicks")]
        public int attackStartTicks;
        // 공격 상태에 들어간 뒤 근접 판정이 끝나기까지의 시간(틱). attackStartTicks보다 커야 한다. Melee만 쓴다
        public int attackEndTicks;
        // 공격 상태에 들어간 뒤 공격 상태가 끝나기까지의 시간(틱). attackEndTicks 이후는 판정 없는 후딜레이다. Melee만 쓴다
        public int attackDurationTicks;
        // 공격 상태에 들어간 뒤 다음 공격 상태에 들어갈 수 있기까지의 시간(틱). 공격 상태가 끝나는 시간보다 짧으면 효과가 없다
        public int attackIntervalTicks;
        // 내보내는 투사체. Ranged와 Throw가 쓴다
        public ProjectileComponent projectile;
        // 근접 판정 구의 반지름(유닛, XZ). Melee만 쓴다
        public float meleeRadius;
        // 근접 판정 구 중심을 플레이어 앞쪽으로 띄우는 거리(유닛). Melee만 쓴다
        public float meleeOffset;
    }
}
