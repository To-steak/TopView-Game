using System;

namespace TopView.Components
{
    // 판이 있는 씬의 흐름 값. 판 시작 때 Definition에서 World로 복사한다. Logic은 쓰지 않는다.
    [Serializable]
    public struct GameFlowRuleComponent
    {
        // 일시정지 화면의 나가기 버튼으로 돌아갈 씬 이름. Build Settings에 등록되어 있어야 한다
        public string menuSceneName;
    }
}
