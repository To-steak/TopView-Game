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
        Loot = 1UL << 1,
        // 회전
        Rotation = 1UL << 2,
        // 근접 판정 값과 진행 시간
        Melee = 1UL << 3,
        // 근접 판정이 이미 맞힌 대상
        MeleeHits = 1UL << 4,
        // 공격의 진영과 피해량
        AttackPayload = 1UL << 5,
        // 직선 투사체의 비행 값
        StraightFlight = 1UL << 6,
        // 직선 투사체의 이동 상태
        StraightMotion = 1UL << 7,
        // 포물선 투사체의 비행 값
        ArcFlight = 1UL << 8,
        // 포물선 투사체의 이동 상태
        ArcMotion = 1UL << 9,
        // 투사체의 피격 판정 크기와 멈추는 환경 레이어
        ProjectileHit = 1UL << 10,
        // 폭발 범위
        Explosion = 1UL << 11,
        // 투사체를 무엇으로 그릴지
        ProjectileView = 1UL << 12
    }
}
