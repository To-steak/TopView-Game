namespace TopView.Entities
{
    // 엔티티 종류마다 가진 컴포넌트 조합. 프로젝트에 어떤 엔티티가 있는지는 여기서 본다.
    // 생성 System과 Core는 칸을 채울 때 이 값을 마스크로 쓴다. 처리하는 System은 자기가 읽는 컴포넌트만 마스크로 고르므로 이 값을 쓰지 않는다.
    public static class EntityArchetype
    {
        // 플레이어
        public const ComponentMask PLAYER = ComponentMask.Position | ComponentMask.Rotation | ComponentMask.Body | ComponentMask.Death | ComponentMask.Health
            | ComponentMask.PlayerMovement | ComponentMask.PlayerVitals | ComponentMask.PlayerAction | ComponentMask.PlayerUpgrade | ComponentMask.PlayerActionState
            | ComponentMask.PlayerMotion | ComponentMask.Mana | ComponentMask.WeaponSlots | ComponentMask.ThrowPreview | ComponentMask.PlayerUpgradeLevels;

        // 적. 보스도 적 종류 하나다
        public const ComponentMask ENEMY = ComponentMask.Position | ComponentMask.Rotation | ComponentMask.Body | ComponentMask.Death | ComponentMask.Health
            | ComponentMask.EnemyKind | ComponentMask.Score | ComponentMask.NavigationMovement | ComponentMask.AttackList | ComponentMask.LootList | ComponentMask.Combat
            | ComponentMask.EnemyActionState | ComponentMask.NavigationAgent;

        // 근접 무기. 탄약을 쓰지 않는다
        public const ComponentMask MELEE_WEAPON = WEAPON | ComponentMask.MeleeAttack | ComponentMask.MeleeView | ComponentMask.AttackInterval | ComponentMask.AttackDuration;

        // 원거리 무기. 탄창과 예비 탄약을 쓰고, 마지막 발이 나가면 공격 상태가 끝난다
        public const ComponentMask RANGED_WEAPON = WEAPON | ComponentMask.StraightAttack | ComponentMask.ProjectileView | ComponentMask.Magazine | ComponentMask.MagazineState
            | ComponentMask.ReserveAmmo | ComponentMask.ReserveAmmoState | ComponentMask.AttackInterval;

        // 투척 무기. 예비 탄약만 쓴다
        public const ComponentMask THROW_WEAPON = WEAPON | ComponentMask.ArcAttack | ComponentMask.Throw | ComponentMask.ProjectileView | ComponentMask.ReserveAmmo
            | ComponentMask.ReserveAmmoState | ComponentMask.AttackDuration;

        // 근접 판정. 근접 공격이 나갈 때 생기고 판정 시간이 끝나면 사라진다
        public const ComponentMask MELEE = ComponentMask.Position | ComponentMask.Melee | ComponentMask.MeleeHits | ComponentMask.AttackPayload;

        // 직선 투사체
        public const ComponentMask STRAIGHT_PROJECTILE = PROJECTILE | ComponentMask.StraightFlight | ComponentMask.StraightMotion;

        // 포물선 투사체
        public const ComponentMask ARC_PROJECTILE = PROJECTILE | ComponentMask.ArcFlight | ComponentMask.ArcMotion;

        // 바닥에 떨어진 전리품
        public const ComponentMask LOOT = ComponentMask.Position | ComponentMask.Loot;

        // 적 스포너. 판 시작 때 씬의 Enemy Spawner Authoring마다 생기고 판이 끝날 때까지 남는다
        public const ComponentMask ENEMY_SPAWNER = ComponentMask.Position | ComponentMask.Rotation | ComponentMask.EnemySpawner | ComponentMask.EnemySpawnerRule
            | ComponentMask.EnemySpawnerState | ComponentMask.SpawnerLink;

        // 플레이어 감지기. 판 시작 때 씬의 Player Detector Authoring마다 생기고 판이 끝날 때까지 남는다
        public const ComponentMask PLAYER_DETECTOR = ComponentMask.Position | ComponentMask.PlayerDetectorRange | ComponentMask.PlayerDetectorState | ComponentMask.PlayerDetectorLinks;

        // 모든 무기가 가진 컴포넌트
        private const ComponentMask WEAPON = ComponentMask.Owner | ComponentMask.Weapon | ComponentMask.UpgradeLevel | ComponentMask.WeaponUpgrade | ComponentMask.AttackState;
        // 모든 투사체가 가진 컴포넌트
        private const ComponentMask PROJECTILE = ComponentMask.Position | ComponentMask.Rotation | ComponentMask.ProjectileHit | ComponentMask.Explosion | ComponentMask.ProjectileView
            | ComponentMask.AttackPayload;
    }
}
