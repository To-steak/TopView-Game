using System;

namespace TopView.Components
{
    // 예비 탄약을 쓰는 무기의 예비 탄약 값. 게임 중의 예비 탄약 수는 ReserveAmmoStateComponent에 있다.
    [Serializable]
    public struct ReserveAmmoComponent
    {
        // 시작할 때 예비 탄약 수
        public int startingReserve;
    }
}
