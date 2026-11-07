namespace TopView.Presentation.ViewContexts
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
        // 투사체 View 실행 상태
        public ProjectileViewContext projectile;
        // 피격 이펙트 실행 상태
        public HitEffectViewContext hitEffect;
        // 월드 글자 실행 상태
        public WorldTextViewContext worldText;
        // 전리품 View 실행 상태
        public LootViewContext loot;
        // 배경음악 실행 상태. Definition이 없으면 null
        public MusicViewContext music;
    }
}
