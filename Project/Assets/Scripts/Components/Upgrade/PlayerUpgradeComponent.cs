using System;

namespace TopView.Components
{
    // 플레이어 강화 값. 생성 때 Definition에서 복사한다. 수치는 Definition의 값에 (1 ± 비율 × 레벨)을 곱해 정한다.
    [Serializable]
    public struct PlayerUpgradeComponent
    {
        // 항목마다 강화할 수 있는 최대 레벨
        public int maxLevel;
        // 0레벨에서 1레벨로 강화하는 비용(골드)
        public int startingCost;
        // 레벨이 1 오를 때마다 늘어나는 비용(골드)
        public int costIncreasePerLevel;
        // 레벨마다 최대 체력이 오르는 비율. 0.1이면 레벨마다 Definition 값의 10%씩 오른다
        public float maxHealthRatioPerLevel;
        // 레벨마다 최대 마나가 오르는 비율
        public float maxManaRatioPerLevel;
        // 레벨마다 달리기 마나 소모량이 줄어드는 비율. 0 아래로는 줄지 않는다
        public float runManaCostRatioPerLevel;
        // 레벨마다 회피 마나 소모량이 줄어드는 비율. 0 아래로는 줄지 않는다
        public float dodgeManaCostRatioPerLevel;
        // 레벨마다 점프 마나 소모량이 줄어드는 비율. 0 아래로는 줄지 않는다
        public float jumpManaCostRatioPerLevel;
        // 레벨마다 무기 피해량이 오르는 비율. 무기마다 레벨을 따로 센다. 무기별 추가 효과의 비율은 WeaponUpgradeRuleComponent에 있다
        public float damageRatioPerLevel;
        // 강화로 줄어든 공격 간격의 최솟값(틱). 처음부터 이보다 짧은 무기는 처음 값을 유지한다
        public int minAttackIntervalTicks;
    }
}
