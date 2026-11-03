namespace TopView.Components
{
    // 전리품 종류
    public enum LootKind
    {
        // 아무것도 떨어뜨리지 않는다. 드롭 표에서만 쓴다
        None,
        // 골드를 더한다
        Gold,
        // 체력을 회복한다
        Health,
        // 원거리 무기의 예비 탄약을 더한다
        Ammo
    }
}
