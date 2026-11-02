using TopView.Components;
using UnityEngine;

namespace TopView.Definitions
{
    // 보스의 값
    [CreateAssetMenu(fileName = "Boss Definition", menuName = "Definitions/Boss Definition")]
    public class BossDefinition : ScriptableObject
    {
        // 체력·공격력·점수 값
        public EnemyStatsComponent stats;
        // 행동 값
        public BossBehaviorComponent behavior;
        // 공격 목록. 공격 상태에 들어갈 때 무작위로 하나를 고른다
        public EnemyAttackEntry[] attacks;
        // BossView가 붙은 프리팹
        public GameObject viewPrefab;
    }
}
