namespace TopView.Components
{
    // 런타임 값. 지금 예비 탄약 수. 탄창이 있는 무기는 재장전할 때 여기서 채우고, 없는 무기는 공격할 때 여기서 쓴다.
    public struct ReserveAmmoStateComponent
    {
        // 예비 탄약 수
        public int reserve;
    }
}
