using System;

namespace TopView.Components
{
    // 적·보스의 체력, 공격력, 점수, 골드 값. 레벨이 오를수록 체력과 공격력이 늘어난다
    [Serializable]
    public struct EnemyStatsComponent
    {
        // 레벨 0일 때의 최대 체력
        public float basicMaxHealth;
        // 레벨 0일 때의 공격력
        public float basicAttackDamage;
        // 레벨 1마다 최대 체력과 공격력에 더하는 값
        public float levelIncrement;
        // 처치 점수. 실제로는 레벨을 곱해 더한다
        public int score;
        // 처치 골드의 기본값. 실제로는 레벨을 곱해 더한다
        public int baseGold;
        // 피격 판정 반지름(유닛, XZ)
        public float hitRadius;
    }
}
