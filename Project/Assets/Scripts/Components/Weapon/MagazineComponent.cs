using System;

namespace TopView.Components
{
    // 탄창에 장전하는 무기의 탄창 값. 게임 중의 장전된 탄약 수는 MagazineStateComponent에 있다.
    [Serializable]
    public struct MagazineComponent
    {
        // 장전할 수 있는 최대 탄약 수. 강화 레벨로 늘어난다
        public int max;
        // 시작할 때 장전된 탄약 수
        public int startingCurrent;
    }
}
