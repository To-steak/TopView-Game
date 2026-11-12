namespace TopView.Commands
{
    // 플레이어 강화 명령. UpgradePresenter가 넣고 PlayerUpgradeSystem이 소비한다.
    public struct PlayerUpgradeCommand
    {
        // 강화 항목
        public PlayerUpgradeCommandType type;
        // Weapon일 때 강화할 무기 번호. 번호는 PlayerWeaponSlotsComponent의 무기 칸 번호와 같다. 다른 항목은 쓰지 않는다
        public int weaponIndex;
    }
}
