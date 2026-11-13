namespace TopView.Components
{
    // 런타임 값. 플레이어의 항목별 강화 레벨. 생성 때 모두 0이다. 무기의 강화 레벨은 무기 엔티티에 있다.
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
    }
}
