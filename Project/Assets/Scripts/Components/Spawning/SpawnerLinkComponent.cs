namespace TopView.Components
{
    // 런타임 값. 플레이어 감지기가 적 스포너를 가리키는 연결 번호. 판을 준비할 때 Core가 스포너마다 씬 순서로 붙이며 바뀌지 않는다.
    public struct SpawnerLinkComponent
    {
        // 연결 번호. 한 판의 스포너마다 다르다
        public int linkIndex;
    }
}
