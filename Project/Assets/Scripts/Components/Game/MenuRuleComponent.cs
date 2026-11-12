using System;

namespace TopView.Components
{
    // 메뉴의 흐름 값. MenuFlow가 시작할 때 Definition에서 복사한다.
    [Serializable]
    public struct MenuRuleComponent
    {
        // 시작 버튼으로 불러올 씬 이름. Build Settings에 등록되어 있어야 한다
        public string worldSceneName;
        // 로딩 화면을 보여 주는 최소 시간(틱). 불러오기가 일찍 끝나도 이 시간이 지나야 들어가기 버튼이 보인다. 0 이하면 최소 시간이 없다
        public int minimumLoadingTicks;
    }
}
