namespace TopView.Components
{
    // 무기 종류. 공격 상태와 탄약 규칙을 정한다
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
