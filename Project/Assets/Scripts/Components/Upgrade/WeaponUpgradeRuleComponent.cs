using System;

namespace TopView.Components
{
    // 무기 하나를 강화할 때 레벨마다 붙는 효과. 피해량이 오르는 비율은 모든 무기가 같아 PlayerUpgradeComponent에 있다
    [Serializable]
    public struct WeaponUpgradeRuleComponent
    {
        // 강화 레벨마다 최대 탄약 수가 오르는 비율. 반올림하며, 늘어난 만큼 탄창에도 바로 넣는다. 0이면 오르지 않는다. Ranged만 쓴다
        public float maxAmmoRatioPerLevel;
        // 강화 레벨마다 공격 간격이 줄어드는 비율. 반올림하며 Player Definition의 최소 공격 간격 아래로는 줄지 않는다. 0이면 줄지 않는다. Ranged와 Melee가 쓴다
        public float attackIntervalRatioPerLevel;
        // 강화 레벨마다 투사체 폭발 반지름이 늘어나는 양(유닛). 0이면 늘지 않는다. Straight는 0레벨 폭발 반지름이 0이어도 1레벨부터 폭발한다. Ranged와 Throw가 쓴다
        public float explosionRadiusIncreasePerLevel;
        // 강화 레벨마다 근접 판정 반지름, 앞쪽으로 띄우는 거리, 임팩트 이펙트 크기가 함께 오르는 비율. 0이면 오르지 않는다. Melee만 쓴다
        public float meleeRangeRatioPerLevel;
    }
}
