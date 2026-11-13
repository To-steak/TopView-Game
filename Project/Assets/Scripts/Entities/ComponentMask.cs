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
        ProjectileView = 1UL << 12,
        // 적 종류 번호
        EnemyKind = 1UL << 13,
        // 처치 점수 값
        Score = 1UL << 14,
        // 몸체 크기
        Body = 1UL << 15,
        // NavMesh 위 이동과 회전 값
        NavigationMovement = 1UL << 16,
        // 사망 상태의 길이와 사라질 때 일어나는 일
        Death = 1UL << 17,
        // 고를 수 있는 공격 목록
        AttackList = 1UL << 18,
        // 드롭 표
        LootList = 1UL << 19,
        // 레벨과 공격력
        Combat = 1UL << 20,
        // 체력
        Health = 1UL << 21,
        // 적의 행동 상태와 공격 진행
        EnemyActionState = 1UL << 22,
        // NavMesh 목적지와 경로
        NavigationAgent = 1UL << 23,
        // 플레이어 이동 값
        PlayerMovement = 1UL << 24,
        // 플레이어 체력과 마나 값
        PlayerVitals = 1UL << 25,
        // 플레이어 행동 상태의 길이
        PlayerAction = 1UL << 26,
        // 플레이어 강화 값
        PlayerUpgrade = 1UL << 27,
        // 플레이어의 행동 상태와 공격 진행. 플레이어를 가리는 데도 쓴다
        PlayerActionState = 1UL << 28,
        // 플레이어 이동 상태
        PlayerMotion = 1UL << 29,
        // 마나
        Mana = 1UL << 30,
        // 든 무기
        WeaponSlots = 1UL << 31,
        // 투척 미리보기
        ThrowPreview = 1UL << 32,
        // 플레이어 강화 레벨
        PlayerUpgradeLevels = 1UL << 33,
        // 이 엔티티를 든 주인
        Owner = 1UL << 34,
        // 모든 무기에 있는 값(피해량, 조준 높이)
        Weapon = 1UL << 35,
        // 강화 레벨
        UpgradeLevel = 1UL << 36,
        // 무기 강화 효과
        WeaponUpgrade = 1UL << 37,
        // 주인이 공격 키를 누를 때 들어갈 행동 상태
        AttackState = 1UL << 38,
        // 근접 공격
        MeleeAttack = 1UL << 39,
        // 근접 판정을 무엇으로 그릴지
        MeleeView = 1UL << 40,
        // 직선 투사체 공격
        StraightAttack = 1UL << 41,
        // 포물선 투사체 공격
        ArcAttack = 1UL << 42,
        // 던지기 전에 착탄점을 정하는 값
        Throw = 1UL << 43,
        // 탄창 값
        Magazine = 1UL << 44,
        // 장전된 탄약 수
        MagazineState = 1UL << 45,
        // 예비 탄약 값
        ReserveAmmo = 1UL << 46,
        // 예비 탄약 수
        ReserveAmmoState = 1UL << 47,
        // 연달아 공격하는 간격
        AttackInterval = 1UL << 48,
        // 공격 상태의 길이
        AttackDuration = 1UL << 49
    }
}
