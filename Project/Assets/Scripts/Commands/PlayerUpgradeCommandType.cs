namespace TopView.Commands
{
    // 플레이어 강화 항목
    public enum PlayerUpgradeCommandType
    {
        // 최대 체력을 올린다
        MaxHealth,
        // 최대 마나를 올린다
        MaxMana,
        // 달리기 마나 소모량을 줄인다
        RunManaCost,
        // 회피 마나 소모량을 줄인다
        DodgeManaCost,
        // 점프 마나 소모량을 줄인다
        JumpManaCost,
        // 무기 하나를 강화한다. 피해량과 그 무기의 추가 효과(WeaponUpgradeRuleComponent의 ~PerLevel 값)가 함께 오른다
        Weapon
    }
}
