namespace TopView.Components
{
    // 런타임 값. 플레이어 감지기가 발동할 적 스포너. 판을 준비할 때 Core가 정하며 바뀌지 않는다.
    public struct PlayerDetectorLinksComponent
    {
        // 플레이어를 감지하면 발동할 적 스포너의 연결 번호(SpawnerLinkComponent.linkIndex) 목록
        public int[] spawnerLinkIndices;
    }
}
