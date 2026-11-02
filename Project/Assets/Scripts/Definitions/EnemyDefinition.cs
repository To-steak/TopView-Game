using TopView.Components;
using UnityEngine;

namespace TopView.Definitions
{
    // 적 한 종류의 값
    [CreateAssetMenu(fileName = "Enemy Definition", menuName = "Definitions/Enemy Definition")]
    public class EnemyDefinition : ScriptableObject
    {
        // 체력·공격력·점수·골드 값
        public EnemyStatsComponent stats;
        // 행동 값
        public EnemyBehaviorComponent behavior;
        // 길찾기 이동 값
        public EnemyNavigationComponent navigation;
        // 공격 목록. 공격 상태에 들어갈 때 무작위로 하나를 고른다
        public EnemyAttackComponent[] attacks;
        // 공격별 ProjectileView가 붙은 투사체 프리팹. 번호는 attacks와 같고, Body 공격 칸은 비워 둔다
        public GameObject[] attackProjectilePrefabs;
        // 공격별 투사체가 맞을 때 재생할 ParticleSystem 프리팹. 번호는 attacks와 같고, 없으면 null
        public GameObject[] attackHitEffectPrefabs;
        // 공격별 투사체 모델이 앞으로 구르는 각속도(도/초). 번호는 attacks와 같고, 0이면 구르지 않는다
        public float[] attackProjectilePitchSpeeds;
        // EnemyView가 붙은 프리팹
        public GameObject viewPrefab;
        // EnemyHealthGaugeView가 붙은 월드 공간 체력 게이지 프리팹
        public GameObject healthGaugePrefab;
        // 체력 게이지를 적 위치에서 띄울 거리(월드)
        public Vector3 healthGaugeOffset;
        // 체력 게이지 색
        public Color healthGaugeColor;
    }
}
