namespace TopView.Components
{
    // 무기 종류. WeaponDefinition에서 무기가 가질 컴포넌트를 고르는 작성용 값이며, 게임 중에는 쓰지 않는다
    public enum WeaponKind
    {
        // 근접 무기. 탄약을 쓰지 않는다
        Melee,
        // 원거리 무기. 장전된 탄약과 예비 탄약을 쓴다
        Ranged,
        // 투척 무기. 예비 탄약만 쓴다
        Throw
    }
}
