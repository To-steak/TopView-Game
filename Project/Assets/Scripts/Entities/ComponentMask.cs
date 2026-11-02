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
        Health = 1UL << 7,
        Combat = 1UL << 8,
        EnemyStats = 1UL << 9,
        AttackList = 1UL << 10,
        BossBehavior = 1UL << 11,
        BossActionState = 1UL << 12,
        EnemyKind = 1UL << 13,
        EnemyBehavior = 1UL << 14,
        EnemyActionState = 1UL << 15,
        NavigationAgent = 1UL << 16,

        // 투사체가 가지는 컴포넌트 조합
        ProjectileArchetype = Position | Rotation | Projectile | ProjectileMotion | AttackPayload,
        // 근접 판정이 가지는 컴포넌트 조합
        MeleeArchetype = Position | Melee | MeleeHits | AttackPayload,
        // 적이 가지는 컴포넌트 조합
        EnemyArchetype = Position | Rotation | Health | Combat | EnemyStats | AttackList | EnemyKind | EnemyBehavior | EnemyActionState | NavigationAgent,
        // 보스가 가지는 컴포넌트 조합
        BossArchetype = Position | Rotation | Health | Combat | EnemyStats | AttackList | BossBehavior | BossActionState
    }
}
