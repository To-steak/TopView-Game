using System;

namespace TopView.Entities
{
    // 공유 저장소의 칸이 가진 컴포넌트 표시. None이면 빈 칸이다.
    [Flags]
    public enum ComponentMask : ulong
    {
        None = 0,
        Position = 1UL << 0,
        Rotation = 1UL << 1,
        Projectile = 1UL << 2,
        ProjectileMotion = 1UL << 3,
        AttackPayload = 1UL << 4,
        Melee = 1UL << 5,
        MeleeHits = 1UL << 6,

        // 투사체가 가지는 컴포넌트 조합
        ProjectileArchetype = Position | Rotation | Projectile | ProjectileMotion | AttackPayload,
        // 근접 판정이 가지는 컴포넌트 조합
        MeleeArchetype = Position | Melee | MeleeHits | AttackPayload
    }
}
