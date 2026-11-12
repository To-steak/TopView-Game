namespace TopView.Components
{
    // 런타임 값. 플레이어의 항목별 강화 레벨. 생성 때 모두 0이다.
    public struct PlayerUpgradeLevelsComponent
    {
        // 최대 체력 레벨
        public int maxHealthLevel;
        // 최대 마나 레벨
        public int maxManaLevel;
        // 달리기 마나 소모량 레벨
        public int runManaCostLevel;
        // 회피 마나 소모량 레벨
        public int dodgeManaCostLevel;
        // 점프 마나 소모량 레벨
        public int jumpManaCostLevel;
        // 무기별 강화 레벨. 피해량과 무기별 추가 효과가 함께 오른다. 번호는 PlayerWeaponSlotsComponent의 무기 칸 번호와 같다. 재사용하는 배열이다
        public int[] weaponLevels;
    }
}
