namespace TopView.Components
{
    // 투사체를 무엇으로 그릴지. 런타임 값으로, 판을 준비할 때 Core가 채운다. 판정과 이동은 읽지 않는다
    public struct ProjectileViewComponent
    {
        // 투사체 View 표의 번호. 음수면 없음
        public int viewIndex;
        // 피격 이펙트 표의 번호. 음수면 없음
        public int hitEffectIndex;
    }
}
