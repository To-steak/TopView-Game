using TopView.Components;
using UnityEngine;

namespace TopView.Definitions
{
    // 적 한 종류의 값
    [CreateAssetMenu(fileName = "Enemy Definition", menuName = "Definitions/Enemy Definition")]
    public class EnemyDefinition : ScriptableObject
    {
        // 최대 체력
        public LevelScalingComponent maxHealth;
        // 공격력. 공격마다 배율을 곱한다
        public LevelScalingComponent attackDamage;
        // 최고 이동 속도(유닛/초)
        public LevelScalingComponent speed;
        // 처치 점수. 계산한 값을 반올림해 더한다
        public LevelScalingComponent score;
        // 몸체 크기
        public BodyComponent body;
        // 이동, 회전 값
        public NavigationMovementComponent movement;
        // 사망 상태의 길이와 사라질 때 일어나는 일
        public DeathComponent death;
        // 공격 목록. 공격 상태에 들어갈 때 무작위로 하나를 고른다
        public EnemyAttackEntry[] attacks;
        // 드롭 표. 사망 연출이 끝날 때 가중치 비율로 항목 하나를 뽑는다
        public EnemyLootComponent[] loots;
        // EnemyView가 붙은 프리팹
        public GameObject viewPrefab;
        // EnemyHealthGaugeView가 붙은 월드 공간 체력 게이지 프리팹
        public GameObject healthGaugePrefab;
        // 화면의 오버레이 체력 바에 체력을 보여 주는지. 보스에 켠다. 켜면 체력 게이지 프리팹이 없어도 경고하지 않고, 이 종류가 살아 있으면 보스 곡으로 바꾼다
        public bool showsOverlayHealthBar;
    }
}
