using System;

namespace TopView.Entities
{
    // 엔티티가 가진 컴포넌트. 칸마다 하나 두며, 켜진 비트의 컴포넌트만 뜻이 있다. None이면 빈 칸이다.
    // System은 처리할 컴포넌트의 비트가 모두 켜진 칸만 돈다. 모든 컴포넌트가 아니라 엔티티마다 있고 없음이 갈리는 것만 비트로 둔다.
    [Flags]
    public enum ComponentMask : ulong
    {
        // 빈 칸
        None = 0,
        // 위치
        Position = 1UL << 0,
        // 바닥에 떨어진 전리품
        Loot = 1UL << 1
    }
}
