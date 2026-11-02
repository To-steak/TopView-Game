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
        Faction = 1UL << 17,
        Mana = 1UL << 18,
        PlayerMovement = 1UL << 19,
        PlayerVitals = 1UL << 20,
        PlayerAction = 1UL << 21,
        PlayerActionState = 1UL << 22,
        PlayerMotion = 1UL << 23,
        WeaponList = 1UL << 24,
        WeaponSlots = 1UL << 25,

        // 적이 가지는 컴포넌트 조합
        EnemyArchetype = Position | Rotation | Health | Faction | Combat | EnemyStats | AttackList | EnemyKind | EnemyBehavior | EnemyActionState | NavigationAgent,
        // 보스가 가지는 컴포넌트 조합
        BossArchetype = Position | Rotation | Health | Faction | Combat | EnemyStats | AttackList | BossBehavior | BossActionState,
        // 플레이어가 가지는 컴포넌트 조합
        PlayerArchetype = Position | Rotation | Health | Faction | Mana | PlayerMovement | PlayerVitals | PlayerAction | PlayerActionState | PlayerMotion | WeaponList | WeaponSlots
    }
}
