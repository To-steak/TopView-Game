namespace TopView.Presentation
{
    // View 쪽 실행 상태를 영역별로 모은다. 판마다 새로 만들고, ViewSystem에는 필요한 영역만 넘긴다.
    public class ViewContext
    {
        // 카메라 실행 상태
        public CameraViewContext camera;
        // 플레이어 View 실행 상태
        public PlayerViewContext player;
        // 적 View와 체력 게이지 실행 상태
        public EnemyViewContext enemy;
        // 보스 View 실행 상태
        public BossViewContext boss;
        // 투사체 View 실행 상태
        public ProjectileViewContext projectile;
        // 피격 이펙트 실행 상태
        public HitEffectViewContext hitEffect;
    }
}
