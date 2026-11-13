namespace TopView.Components
{
    // 근접 판정을 무엇으로 그릴지. 런타임 값으로, 판을 준비할 때 Core가 채운다. 판정은 읽지 않는다
    public struct MeleeViewComponent
    {
        // 근접 판정이 시작될 때 재생할 피격 이펙트 표의 번호. 음수면 없음
        public int impactEffectIndex;
    }
}
