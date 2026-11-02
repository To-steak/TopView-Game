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
        Mana = 1UL << 18,
        PlayerMovement = 1UL << 19,
        PlayerVitals = 1UL << 20,
        PlayerAction = 1UL << 21,
        PlayerActionState = 1UL << 22,
        PlayerMotion = 1UL << 23,
        WeaponList = 1UL << 24,
        WeaponSlots = 1UL << 25,

        // 플레이어가 가지는 컴포넌트 조합
        PlayerArchetype = Position | Rotation | Health | Mana | PlayerMovement | PlayerVitals | PlayerAction | PlayerActionState | PlayerMotion | WeaponList | WeaponSlots
    }
}
