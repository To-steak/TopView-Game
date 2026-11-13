using TopView.Components;
using UnityEngine;

namespace TopView.Definitions
{
    // 무기 하나의 종류와 값. 종류에 따라 무기가 가질 컴포넌트를 Core가 고르며, 종류가 쓰지 않는 칸은 읽지 않는다.
    [CreateAssetMenu(fileName = "Weapon Definition", menuName = "Definitions/Weapon Definition")]
    public class WeaponDefinition : ScriptableObject
    {
        // 무기 종류. 무기가 가질 컴포넌트와 공격 키를 누를 때 들어갈 행동 상태를 정한다
        public WeaponKind kind;
        // 모든 무기에 있는 값
        public WeaponComponent weapon;
        // 강화할 때 레벨마다 붙는 효과
        public WeaponUpgradeRuleComponent upgrade;
        // 연달아 공격하는 간격. Melee와 Ranged가 쓴다
        public AttackIntervalComponent attackInterval;
        // 공격 상태의 길이. Melee와 Throw가 쓴다. Ranged는 마지막 발이 나가면 끝난다
        public AttackDurationComponent attackDuration;
        // 근접 공격. Melee만 쓴다
        public MeleeAttackComponent meleeAttack;
        // 직선 투사체 공격. Ranged만 쓴다
        public StraightAttackComponent straightAttack;
        // 포물선 투사체 공격. Throw만 쓴다
        public ArcAttackComponent arcAttack;
        // 던지기 전에 착탄점을 정하는 값. Throw만 쓴다
        public ThrowComponent throwing;
        // 탄창 값. Ranged만 쓴다
        public MagazineComponent magazine;
        // 예비 탄약 값. Ranged와 Throw가 쓴다
        public ReserveAmmoComponent reserveAmmo;
        // 플레이어 무기 소켓에 붙일 모델 프리팹
        public GameObject modelPrefab;
        // ProjectileView가 붙은 투사체 프리팹. Ranged와 Throw가 쓴다
        public GameObject projectilePrefab;
        // 투사체가 맞거나 폭발할 때 재생할 ParticleSystem 프리팹. 없으면 null. 루트에 AudioSource가 있으면 그 소리도 함께 재생한다
        public GameObject hitEffectPrefab;
        // 근접 판정이 시작될 때 판정 원의 중심에서 재생할 ParticleSystem 프리팹. Melee만 쓰며, 없으면 null. 루트에 AudioSource가 있으면 그 소리도 함께 재생한다
        public GameObject impactEffectPrefab;
    }
}
