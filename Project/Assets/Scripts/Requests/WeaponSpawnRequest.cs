using TopView.Components;
using TopView.Entities;

namespace TopView.Requests
{
    // 무기를 만들어 달라는 요청. 판을 준비할 때 Core가 무기마다 owner와 slotIndex를 비워 만들고, 플레이어가 생길 때 PlayerSpawnSystem이 채워 넣는다.
    // mask에 켜진 컴포넌트만 뜻이 있고, 무기 칸에 그대로 옮긴다.
    public struct WeaponSpawnRequest
    {
        // 이 무기를 들 주인
        public EntityIdentifier owner;
        // 주인의 무기 목록에서 이 무기가 놓일 칸 번호
        public int slotIndex;
        // 무기가 가질 컴포넌트. Owner와 상태 컴포넌트(MagazineState, ReserveAmmoState)도 켜 둔다
        public ComponentMask mask;
        // 모든 무기에 있는 값
        public WeaponComponent weapon;
        // 무기 강화 효과
        public WeaponUpgradeRuleComponent upgrade;
        // 주인이 공격 키를 누를 때 들어갈 행동 상태
        public AttackStateComponent attackState;
        // 근접 공격
        public MeleeAttackComponent meleeAttack;
        // 근접 판정을 무엇으로 그릴지
        public MeleeViewComponent meleeView;
        // 직선 투사체 공격
        public StraightAttackComponent straightAttack;
        // 포물선 투사체 공격
        public ArcAttackComponent arcAttack;
        // 던지기 전에 착탄점을 정하는 값
        public ThrowComponent throwing;
        // 투사체를 무엇으로 그릴지
        public ProjectileViewComponent projectileView;
        // 탄창 값
        public MagazineComponent magazine;
        // 예비 탄약 값
        public ReserveAmmoComponent reserveAmmo;
        // 연달아 공격하는 간격
        public AttackIntervalComponent attackInterval;
        // 공격 상태의 길이
        public AttackDurationComponent attackDuration;
    }
}
