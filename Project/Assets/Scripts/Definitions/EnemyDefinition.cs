using TopView.Components;
using UnityEngine;

namespace TopView.Definitions
{
    // 적 한 종류의 값
    [CreateAssetMenu(fileName = "Enemy Definition", menuName = "Definitions/Enemy Definition")]
    public class EnemyDefinition : ScriptableObject
    {
        // 체력·공격력·이동 속도·점수·골드 값
        public EnemyStatsComponent stats;
        // 행동 값
        public EnemyBehaviorComponent behavior;
        // 공격 목록. 공격 상태에 들어갈 때 무작위로 하나를 고른다
        public EnemyAttackEntry[] attacks;
        // EnemyView가 붙은 프리팹
        public GameObject viewPrefab;
        // EnemyHealthGaugeView가 붙은 월드 공간 체력 게이지 프리팹
        public GameObject healthGaugePrefab;
    }
}
