namespace TopView.Components
{
    // 런타임 값. 무기 하나의 지금 탄약 수. 시작값과 최대 탄약 수는 AmmoComponent에 있다.
    public struct AmmoStateComponent
    {
        // 장전된 탄약 수. Ranged만 쓴다
        public int current;
        // 예비 탄약 수. Ranged와 Throw가 쓴다
        public int reserve;
    }
}
