using System;

namespace TopView.Components
{
    // 적의 체력, 공격력, 이동 속도, 점수, 골드 값. 각 값은 기본값 + 레벨 × 증가량이다
    [Serializable]
    public struct EnemyStatsComponent
    {
        // 레벨 0일 때의 최대 체력
        public float baseMaxHealth;
        // 레벨 0일 때의 공격력
        public float baseAttackDamage;
        // 레벨 0일 때의 최고 이동 속도(유닛/초)
        public float baseSpeed;
        // 레벨 0일 때의 처치 점수
        public int baseScore;
        // 레벨 0일 때의 처치 골드
        public int baseGold;
        // 레벨 1마다 최대 체력에 더하는 값
        public float maxHealthIncrement;
        // 레벨 1마다 공격력에 더하는 값
        public float attackDamageIncrement;
        // 레벨 1마다 최고 이동 속도에 더하는 값(유닛/초)
        public float speedIncrement;
        // 레벨 1마다 처치 점수에 더하는 값
        public int scoreIncrement;
        // 레벨 1마다 처치 골드에 더하는 값
        public int goldIncrement;
        // 몸체 반지름(유닛, XZ). 피격 판정, 플레이어 이동 막기, 적끼리 밀어내기에 쓴다
        public float hitRadius;
    }
}
