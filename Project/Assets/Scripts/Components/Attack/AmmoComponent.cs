using System;

namespace TopView.Components
{
    // 무기 하나의 탄약 값. 게임 중의 탄약 수는 AmmoStateComponent에 있다
    [Serializable]
    public struct AmmoComponent
    {
        // 장전할 수 있는 최대 탄약 수. Ranged만 쓴다
        public int max;
        // 시작할 때 장전된 탄약 수. Ranged만 쓴다
        public int startingCurrent;
        // 시작할 때 예비 탄약 수. Ranged와 Throw가 쓴다
        public int startingReserve;
    }
}
